using System.Collections.Immutable;
using AwesomeAssertions;
using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

[Collection(ClusterCollection.Name)]
public class PersistentCacheTestGrainTests
{
  private readonly ClusterFixture _fixture;
  private const string DefaultData = "persistent in cluster cache";

  public PersistentCacheTestGrainTests(ClusterFixture fixture)
  {
    _fixture = fixture;
  }

  private GrainId GetGrainId(string key)
  {
    return OrleansHelpers.GetGrainIdFactory(_fixture).CreateGrainId<IPersistentCacheTestGrain>(key);
  }

  private async Task<GrainState<CacheState<CacheTestValue>>> GetStateAsync(GrainId grainId)
  {
    GrainState<CacheState<CacheTestValue>> state = new();
    await OrleansHelpers.GetDefaultGrainStorage(_fixture).ReadStateAsync(nameof(PersistentCacheTestGrain), grainId, state);
    return state;
  }

  private async Task<GrainState<CacheState<CacheTestValue>>> SetStateAsync(GrainId grainId, CacheState<CacheTestValue> cacheState)
  {
    GrainState<CacheState<CacheTestValue>> state = new(cacheState);
    await OrleansHelpers.GetDefaultGrainStorage(_fixture).WriteStateAsync(nameof(PersistentCacheTestGrain), grainId, state);
    return state;
  }

  [Fact]
  public async Task GetOrCreateAsync_ReturnsGeneratedValue_WhenNotSetAsync()
  {
    var grainId = GetGrainId("GetOrCreate_ReturnsGeneratedValue");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    var result = await grain.GetOrCreateAsync(CancellationToken.None);
    result.Data.Should().Be(DefaultData);
  }

  [Fact]
  public async Task CreateAsync_Then_GetOrCreateAsync_ReturnsValueAsync()
  {
    var grainId = GetGrainId("Create_Then_GetOrCreate");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    // Force creation
    var created = await grain.CreateAsync(CancellationToken.None);
    created.Data.Should().Be(DefaultData);
    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be(DefaultData);

    // Then ensure subsequent GetOrCreate returns value (cached)
    var fetched = await grain.GetOrCreateAsync(CancellationToken.None);
    fetched.Data.Should().Be(DefaultData);
    state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be(DefaultData);
  }

  [Fact]
  public async Task SetAsync_Then_TryGetAsync_ReturnsValueAsync()
  {
    var grainId = GetGrainId("Set_Then_TryGet");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    await grain.SetAsync(new CacheTestValue() { Data = "custom-value" }, CancellationToken.None, null);

    var (found, value) = await grain.TryGetAsync(CancellationToken.None);
    found.Should().BeTrue();
    value.Should().NotBeNull();
    value.Data.Should().Be("custom-value");
    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be("custom-value");
  }

  [Fact]
  public async Task RemoveAsync_RemovesValueSoTryGetReturnsNullAsync()
  {
    var grainId = GetGrainId("Remove_RemovesValue");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    await grain.SetAsync(new CacheTestValue() { Data = "to-be-removed" }, CancellationToken.None, null);

    // ensure set
    var (foundBefore, _) = await grain.TryGetAsync(CancellationToken.None);
    foundBefore.Should().BeTrue();

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be("to-be-removed");

    // remove and verify
    await grain.RemoveAsync(CancellationToken.None);
    var (foundAfter, valueAfter) = await grain.TryGetAsync(CancellationToken.None);
    foundAfter.Should().BeFalse();
    valueAfter.Should().BeNull();
    var stateAfterRemove = await GetStateAsync(grainId);
    stateAfterRemove.Should().NotBeNull();
    stateAfterRemove.RecordExists.Should().BeFalse();
    stateAfterRemove.State.LastAccessed.Should().Be(DateTimeOffset.MinValue);
  }

  [Fact]
  public async Task RefreshAsync_ExtendsSlidingLifetime_WhenNotExpiredAsync()
  {
    var grainId = GetGrainId("Refresh_Extends");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: default,
        SlidingExpiration: TimeSpan.FromMilliseconds(750)
    );

    await grain.SetAsync(new CacheTestValue() { Data = "refresh-test" }, CancellationToken.None, options);

    // Wait a bit but not until expiration
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    // Refresh should extend lifetime
    var refreshed = await grain.RefreshAsync(CancellationToken.None);
    refreshed.Should().BeTrue();

    // Wait again beyond original remaining time but within refreshed lifetime
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    var (found, value) = await grain.TryGetAsync(CancellationToken.None);
    found.Should().BeTrue();
    value.Should().NotBeNull();
    value.Data.Should().Be("refresh-test");
    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be("refresh-test");
  }

  [Fact]
  public async Task RefreshAsync_DoesNotExtendAbsoluteLifetime_WhenNotExpiredAsync()
  {
    var grainId = GetGrainId("Refresh_DoesNotExtend");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMilliseconds(750),
        SlidingExpiration: default
    );

    await grain.SetAsync(new CacheTestValue() { Data = "refresh-test" }, CancellationToken.None, options);

    // Wait a bit but not until expiration
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    // Refresh should extend lifetime
    var refreshed = await grain.RefreshAsync(CancellationToken.None);
    refreshed.Should().BeTrue();

    // Wait again beyond original remaining time but within refreshed lifetime
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    var (foundAfter, valueAfter) = await grain.TryGetAsync(CancellationToken.None);
    foundAfter.Should().BeFalse();
    valueAfter.Should().BeNull();

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.RecordExists.Should().BeFalse();
    state.State.LastAccessed.Should().Be(DateTimeOffset.MinValue);
  }

  [Fact]
  public async Task RefreshAsync_Removes_WhenExpiredAsync()
  {
    var grainId = GetGrainId("Refresh_Removes_WhenExpired");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);

    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMilliseconds(50),
        SlidingExpiration: default
    );

    await grain.SetAsync(new CacheTestValue() { Data = "refresh-test" }, CancellationToken.None, options);

    // Wait for the entry to expire
    await Task.Delay(150);

    // Refresh should detect expiration and remove the entry
    var refreshed = await grain.RefreshAsync(CancellationToken.None);
    refreshed.Should().BeFalse();

    var (found, value) = await grain.TryGetAsync(CancellationToken.None);
    found.Should().BeFalse();
    value.Should().BeNull();

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.RecordExists.Should().BeFalse();
    state.State.LastAccessed.Should().Be(DateTimeOffset.MinValue);
  }

  [Fact]
  public async Task PeekAsync_DoesNotExtendSlidingLifetimeAsync()
  {
    var grainId = GetGrainId("Peek_DoesNotExtend");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: default,
        SlidingExpiration: TimeSpan.FromMilliseconds(750)
    );

    await grain.SetAsync(new CacheTestValue() { Data = "peek-test" }, CancellationToken.None, options);

    // Wait a bit but not until expiration
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    // Refresh should extend lifetime
    var (found, value) = await grain.TryPeekAsync(CancellationToken.None);
    found.Should().BeTrue();
    value.Should().NotBeNull();
    value.Data.Should().Be("peek-test");

    // Wait again beyond original remaining time but within refreshed lifetime
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    var (foundAfter, valueAfter) = await grain.TryGetAsync(CancellationToken.None);
    foundAfter.Should().BeFalse();
    valueAfter.Should().BeNull();

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.RecordExists.Should().BeFalse();
    state.State.LastAccessed.Should().Be(DateTimeOffset.MinValue);
  }

  [Fact]
  public async Task CachedValue_Expires_AfterAbsoluteExpirationRelativeToNowAsync()
  {
    var grainId = GetGrainId("Expires_After_AbsoluteExpirationRelativeToNow");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMilliseconds(100),
        SlidingExpiration: default
    );
    var value = await grain.GetOrCreateAsync(CancellationToken.None, options);
    value.Data.Should().Be(DefaultData);

    // Wait for expiration (use a little buffer)
    await Task.Delay(TimeSpan.FromMilliseconds(200));

    var (found, afterValue) = await grain.TryGetAsync(CancellationToken.None);
    found.Should().BeFalse();
    afterValue.Should().BeNull();

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.RecordExists.Should().BeFalse();
    state.State.LastAccessed.Should().Be(DateTimeOffset.MinValue);
  }

  [Fact]
  public async Task OnActivate_Removes_StaleStateAsync()
  {
    var grainId = GetGrainId("OnActivate_Removes_StaleState");
    CacheTestValue data = new() { Data = "stale-data" };
    var cacheState = new CacheState<CacheTestValue>
    {
      Value = data,
      AbsoluteExpiration = DateTimeOffset.UtcNow.AddMilliseconds(-500),
      LastAccessed = DateTimeOffset.UtcNow.AddMilliseconds(-1000)
    };
    await SetStateAsync(grainId, cacheState);
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);

    var (foundAfter, valueAfter) = await grain.TryGetAsync(CancellationToken.None);
    foundAfter.Should().BeFalse();
    valueAfter.Should().BeNull();
    var stateAfterRemove = await GetStateAsync(grainId);
    stateAfterRemove.Should().NotBeNull();
    stateAfterRemove.RecordExists.Should().BeFalse();
    stateAfterRemove.State.LastAccessed.Should().Be(DateTimeOffset.MinValue);
  }

  [Fact]
  public async Task OnActivate_Keeps_ValidStateAsync()
  {
    var grainId = GetGrainId("OnActivate_Keeps_ValidState");
    CacheTestValue data = new() { Data = "valid-data" };
    var cacheState = new CacheState<CacheTestValue>
    {
      Value = data,
      AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(3600),
      LastAccessed = DateTimeOffset.UtcNow.AddMilliseconds(-1000)
    };
    await SetStateAsync(grainId, cacheState);
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);

    var (found, value) = await grain.TryGetAsync(CancellationToken.None);
    found.Should().BeTrue();
    value.Should().NotBeNull();
    value.Data.Should().Be("valid-data");
    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be("valid-data");
  }

  [Fact]
  public async Task State_IsNotSavedAfterGet_IfDoesntHaveSlidingExpirationAsync()
  {
    var grainId = GetGrainId("State_IsNotSavedAfterGet_IfDoesntHaveSlidingExpiration");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    var data = ImmutableArray.Create<byte>(1, 2, 3, 4);
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMinutes(5),
        SlidingExpiration: default
    );

    await grain.GetOrCreateAsync(CancellationToken.None, options);

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be(DefaultData);
    var lastAccessed = state.State.LastAccessed;

    var fetched = await grain.GetOrCreateAsync(CancellationToken.None, options);
    fetched.Should().NotBeNull();
    fetched.Data.Should().Be(DefaultData);
    var stateAfterGet = await GetStateAsync(grainId);
    stateAfterGet.Should().NotBeNull();
    stateAfterGet.State.Value.Data.Should().Be(DefaultData);
    stateAfterGet.State.LastAccessed.Should().Be(lastAccessed);
  }

  [Fact]
  public async Task State_IsNotSavedAfterRefresh_IfDoesntHaveSlidingExpirationAsync()
  {
    var grainId = GetGrainId("State_IsNotSavedAfterRefresh_IfDoesntHaveSlidingExpiration");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    var data = ImmutableArray.Create<byte>(1, 2, 3, 4);
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMinutes(5),
        SlidingExpiration: default
    );

    await grain.GetOrCreateAsync(CancellationToken.None, options);

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be(DefaultData);
    var lastAccessed = state.State.LastAccessed;

    await grain.RefreshAsync(CancellationToken.None);
    var stateAfterRefresh = await GetStateAsync(grainId);
    stateAfterRefresh.Should().NotBeNull();
    stateAfterRefresh.State.Value.Data.Should().Be(DefaultData);
    stateAfterRefresh.State.LastAccessed.Should().Be(lastAccessed);
  }

  [Fact]
  public async Task State_IsNotSavedAfterTryGet_IfDoesntHaveSlidingExpirationAsync()
  {
    var grainId = GetGrainId("State_IsNotSavedAfterTryGet_IfDoesntHaveSlidingExpiration");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    var data = ImmutableArray.Create<byte>(1, 2, 3, 4);
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMinutes(5),
        SlidingExpiration: default
    );

    await grain.GetOrCreateAsync(CancellationToken.None, options);

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be(DefaultData);
    var lastAccessed = state.State.LastAccessed;

    var (isFetched, fetched) = await grain.TryGetAsync(CancellationToken.None);
    isFetched.Should().BeTrue();
    fetched.Should().NotBeNull();
    fetched.Data.Should().Be(DefaultData);
    var stateAfterGet = await GetStateAsync(grainId);
    stateAfterGet.Should().NotBeNull();
    stateAfterGet.State.Value.Data.Should().Be(DefaultData);
    stateAfterGet.State.LastAccessed.Should().Be(lastAccessed);
  }

  [Fact]
  public async Task State_IsSavedAfterGet_IfHasSlidingExpirationAsync()
  {
    var grainId = GetGrainId("State_IsSavedAfterGet_IfHasSlidingExpiration");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    var data = ImmutableArray.Create<byte>(1, 2, 3, 4);
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMinutes(5),
        SlidingExpiration: TimeSpan.FromMinutes(2)
    );

    await grain.GetOrCreateAsync(CancellationToken.None, options);

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be(DefaultData);
    var lastAccessed = state.State.LastAccessed;

    var fetched = await grain.GetOrCreateAsync(CancellationToken.None, options);
    fetched.Should().NotBeNull();
    fetched.Data.Should().Be(DefaultData);
    var stateAfterGet = await GetStateAsync(grainId);
    stateAfterGet.Should().NotBeNull();
    stateAfterGet.State.Value.Data.Should().Be(DefaultData);
    stateAfterGet.State.LastAccessed.Should().BeAfter(lastAccessed);
  }

  [Fact]
  public async Task State_IsSavedAfterRefresh_IfHasSlidingExpirationAsync()
  {
    var grainId = GetGrainId("State_IsSavedAfterRefresh_IfHasSlidingExpiration");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    var data = ImmutableArray.Create<byte>(1, 2, 3, 4);
    var options = new CacheGrainEntryOptions(
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMinutes(5),
        SlidingExpiration: TimeSpan.FromMinutes(2));

    await grain.GetOrCreateAsync(CancellationToken.None, options);

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be(DefaultData);
    var lastAccessed = state.State.LastAccessed;

    await grain.RefreshAsync(CancellationToken.None);
    var stateAfterRefresh = await GetStateAsync(grainId);
    stateAfterRefresh.Should().NotBeNull();
    stateAfterRefresh.State.Value.Data.Should().Be(DefaultData);
    stateAfterRefresh.State.LastAccessed.Should().BeAfter(lastAccessed);
  }

  [Fact]
  public async Task State_IsSavedAfterTryGet_IfHasSlidingExpirationAsync()
  {
    var grainId = GetGrainId("State_IsSavedAfterTryGet_IfHasSlidingExpiration");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentCacheTestGrain>(grainId);
    var data = ImmutableArray.Create<byte>(1, 2, 3, 4);
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMinutes(5),
        SlidingExpiration: TimeSpan.FromMinutes(2)
    );

    await grain.GetOrCreateAsync(CancellationToken.None, options);

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be(DefaultData);
    var lastAccessed = state.State.LastAccessed;

    var (isFetched, fetched) = await grain.TryGetAsync(CancellationToken.None);
    isFetched.Should().BeTrue();
    fetched.Should().NotBeNull();
    fetched.Data.Should().Be(DefaultData);
    var stateAfterGet = await GetStateAsync(grainId);
    stateAfterGet.Should().NotBeNull();
    stateAfterGet.State.Value.Data.Should().Be(DefaultData);
    stateAfterGet.State.LastAccessed.Should().BeAfter(lastAccessed);
  }
}

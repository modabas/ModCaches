using AwesomeAssertions;
using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

[Collection(ClusterCollection.Name)]
public class PersistentInClusterCacheTestGrainWithCreateArgsTests
{
  private readonly ClusterFixture _fixture;
  private const string _defaultData = "persistent in cluster cache";

  public PersistentInClusterCacheTestGrainWithCreateArgsTests(ClusterFixture fixture)
  {
    _fixture = fixture;
  }

  private GrainId GetGrainId(string key)
  {
    return OrleansHelpers.GetGrainIdFactory(_fixture).CreateGrainId<IPersistentInClusterCacheTestGrainWithCreateArgs>(key);
  }

  private async Task<GrainState<InClusterCacheState<InClusterTestCacheState>>> GetStateAsync(GrainId grainId)
  {
    GrainState<InClusterCacheState<InClusterTestCacheState>> state = new();
    await OrleansHelpers.GetDefaultGrainStorage(_fixture).ReadStateAsync(nameof(PersistentInClusterCacheTestGrainWithCreateArgs), grainId, state);
    return state;
  }

  private async Task<GrainState<InClusterCacheState<InClusterTestCacheState>>> SetStateAsync(GrainId grainId, InClusterCacheState<InClusterTestCacheState> cacheState)
  {
    GrainState<InClusterCacheState<InClusterTestCacheState>> state = new(cacheState);
    await OrleansHelpers.GetDefaultGrainStorage(_fixture).WriteStateAsync(nameof(PersistentInClusterCacheTestGrainWithCreateArgs), grainId, state);
    return state;
  }

  [Fact]
  public async Task GetOrCreateAsync_ReturnsGeneratedValue_WhenNotSetAsync()
  {
    var grainId = GetGrainId("GetOrCreate_ReturnsGeneratedValue");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentInClusterCacheTestGrainWithCreateArgs>(grainId);
    var result = await grain.GetOrCreateAsync(2, CancellationToken.None);
    result.Data.Should().Be($"{_defaultData} 2");
  }

  [Fact]
  public async Task CreateAsync_Then_GetOrCreateAsync_ReturnsValueAsync()
  {
    var grainId = GetGrainId("Create_Then_GetOrCreate");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentInClusterCacheTestGrainWithCreateArgs>(grainId);
    // Force creation
    var created = await grain.CreateAsync(3, CancellationToken.None);
    created.Data.Should().Be($"{_defaultData} 3");
    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be($"{_defaultData} 3");

    // Then ensure subsequent GetOrCreate returns value (cached)
    var fetched = await grain.GetOrCreateAsync(4, CancellationToken.None);
    fetched.Data.Should().Be($"{_defaultData} 3");
    state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be($"{_defaultData} 3");
  }

  [Fact]
  public async Task SetAsync_Then_TryGetAsync_ReturnsValueAsync()
  {
    var grainId = GetGrainId("Set_Then_TryGet");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentInClusterCacheTestGrainWithCreateArgs>(grainId);
    await grain.SetAsync(new InClusterTestCacheState() { Data = "custom-value" }, CancellationToken.None, null);

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
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentInClusterCacheTestGrainWithCreateArgs>(grainId);
    await grain.SetAsync(new InClusterTestCacheState() { Data = "to-be-removed" }, CancellationToken.None, null);

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
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentInClusterCacheTestGrainWithCreateArgs>(grainId);
    var options = new InClusterCacheEntryOptions
    {
      SlidingExpiration = TimeSpan.FromMilliseconds(750)
    };

    await grain.SetAsync(new InClusterTestCacheState() { Data = "refresh-test" }, CancellationToken.None, options);

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
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentInClusterCacheTestGrainWithCreateArgs>(grainId);
    var options = new InClusterCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(750)
    };

    await grain.SetAsync(new InClusterTestCacheState() { Data = "refresh-test" }, CancellationToken.None, options);

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
  public async Task PeekAsync_DoesNotExtendSlidingLifetimeAsync()
  {
    var grainId = GetGrainId("Peek_DoesNotExtend");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentInClusterCacheTestGrainWithCreateArgs>(grainId);
    var options = new InClusterCacheEntryOptions
    {
      SlidingExpiration = TimeSpan.FromMilliseconds(750)
    };

    await grain.SetAsync(new InClusterTestCacheState() { Data = "peek-test" }, CancellationToken.None, options);

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
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentInClusterCacheTestGrainWithCreateArgs>(grainId);
    var options = new InClusterCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(100)
    };
    var value = await grain.GetOrCreateAsync(5, CancellationToken.None, options);
    value.Data.Should().Be($"{_defaultData} 5");

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
    InClusterTestCacheState data = new() { Data = "discard-stale-data" };
    var cacheState = new InClusterCacheState<InClusterTestCacheState>
    {
      Value = data,
      AbsoluteExpiration = DateTimeOffset.UtcNow.AddMilliseconds(-500),
      LastAccessed = DateTimeOffset.UtcNow.AddMilliseconds(-1000)
    };
    await SetStateAsync(grainId, cacheState);
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentInClusterCacheTestGrainWithCreateArgs>(grainId);

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
    InClusterTestCacheState data = new() { Data = "keep-valid-data" };
    var cacheState = new InClusterCacheState<InClusterTestCacheState>
    {
      Value = data,
      AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(3600),
      LastAccessed = DateTimeOffset.UtcNow.AddMilliseconds(-1000)
    };
    await SetStateAsync(grainId, cacheState);
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentInClusterCacheTestGrainWithCreateArgs>(grainId);

    var (found, value) = await grain.TryGetAsync(CancellationToken.None);
    found.Should().BeTrue();
    value.Should().NotBeNull();
    value.Data.Should().Be("keep-valid-data");
    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Data.Should().Be("keep-valid-data");
  }
}

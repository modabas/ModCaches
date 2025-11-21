using AwesomeAssertions;
using ModCaches.Orleans.Abstractions.Cluster;

namespace ModCaches.Orleans.Server.Tests.Cluster;

[Collection(ClusterCollection.Name)]
public class VolatileCacheTestGrainTests
{
  private readonly ClusterFixture _fixture;

  public VolatileCacheTestGrainTests(ClusterFixture fixture)
  {
    _fixture = fixture;
  }

  [Fact]
  public async Task SetAndWriteAsync_ReturnsWrittenValueAsync()
  {
    var defaultData = "default-data";
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrain>("SetAndWriteAsync_ReturnsWrittenValue");
    var result = await grain.SetAndWriteAsync(defaultData, CancellationToken.None);
    result.IsOk.Should().BeTrue();
    result.Value.Should().Be("write-through " + defaultData);
  }

  [Fact]
  public async Task GetOrCreateAsync_ReturnsGeneratedValue_WhenNotSetAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrain>("GetOrCreate_ReturnsGeneratedValue");
    var result = await grain.GetOrCreateAsync(CancellationToken.None);
    result.IsOk.Should().BeTrue();
    result.Value.Should().Be("volatile in cluster cache");
  }

  [Fact]
  public async Task CreateAsync_Then_GetOrCreateAsync_ReturnsValueAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrain>("Create_Then_GetOrCreate");
    // Force creation
    var created = await grain.CreateAsync(CancellationToken.None);
    created.IsOk.Should().BeTrue();
    created.Value.Should().Be("volatile in cluster cache");

    // Then ensure subsequent GetOrCreate returns value (cached)
    var fetched = await grain.GetOrCreateAsync(CancellationToken.None);
    fetched.IsOk.Should().BeTrue();
    fetched.Value.Should().Be("volatile in cluster cache");
  }

  [Fact]
  public async Task SetAsync_Then_TryGetAsync_ReturnsValueAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrain>("Set_Then_TryGet");
    await grain.SetAsync("custom-value", CancellationToken.None, null);

    var fetched = await grain.GetAsync(CancellationToken.None);
    fetched.IsOk.Should().BeTrue();
    fetched.Value.Should().NotBeNull();
    fetched.Value.Should().Be("custom-value");
  }

  [Fact]
  public async Task RemoveAsync_RemovesValueSoTryGetReturnsNullAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrain>("Remove_RemovesValue");
    await grain.SetAsync("to-be-removed", CancellationToken.None, null);

    // ensure set
    var fetchedBefore = await grain.GetAsync(CancellationToken.None);
    fetchedBefore.IsOk.Should().BeTrue();

    // remove and verify
    await grain.RemoveAsync(CancellationToken.None);
    var fetchedAfter = await grain.GetAsync(CancellationToken.None);
    fetchedAfter.IsOk.Should().BeFalse();
    fetchedAfter.Value.Should().BeNull();
  }

  [Fact]
  public async Task RefreshAsync_ExtendsSlidingLifetime_WhenNotExpiredAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrain>("Refresh_Extends");
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: default,
        SlidingExpiration: TimeSpan.FromMilliseconds(750)
    );

    await grain.SetAsync("refresh-test", CancellationToken.None, options);

    // Wait a bit but not until expiration
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    // Refresh should extend lifetime
    var refreshed = await grain.RefreshAsync(CancellationToken.None);
    refreshed.IsOk.Should().BeTrue();

    // Wait again beyond original remaining time but within refreshed lifetime
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    var fetched = await grain.GetAsync(CancellationToken.None);
    fetched.IsOk.Should().BeTrue();
    fetched.Value.Should().NotBeNull();
    fetched.Value.Should().Be("refresh-test");
  }

  [Fact]
  public async Task RefreshAsync_DoesNotExtendAbsoluteLifetime_WhenNotExpiredAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrain>("Refresh_DoesNotExtend");
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMilliseconds(750),
        SlidingExpiration: default
    );

    await grain.SetAsync("refresh-test", CancellationToken.None, options);

    // Wait a bit but not until expiration
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    // Refresh should extend lifetime
    var refreshed = await grain.RefreshAsync(CancellationToken.None);
    refreshed.IsOk.Should().BeTrue();

    // Wait again beyond original remaining time but within refreshed lifetime
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    var fetchedAfter = await grain.GetAsync(CancellationToken.None);
    fetchedAfter.IsOk.Should().BeFalse();
    fetchedAfter.Value.Should().BeNull();
  }

  [Fact]
  public async Task PeekAsync_DoesNotExtendSlidingLifetimeAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrain>("Peek_DoesNotExtend");
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: default,
        SlidingExpiration: TimeSpan.FromMilliseconds(750)
    );

    await grain.SetAsync("peek-test", CancellationToken.None, options);

    // Wait a bit but not until expiration
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    // Refresh should extend lifetime
    var fetched = await grain.PeekAsync(CancellationToken.None);
    fetched.IsOk.Should().BeTrue();
    fetched.Value.Should().NotBeNull();
    fetched.Value.Should().Be("peek-test");

    // Wait again beyond original remaining time but within refreshed lifetime
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    var fetchedAfter = await grain.GetAsync(CancellationToken.None);
    fetchedAfter.IsOk.Should().BeFalse();
    fetchedAfter.Value.Should().BeNull();
    fetchedAfter.Value.Should().BeNull();
  }

  [Fact]
  public async Task CachedValue_Expires_AfterAbsoluteExpirationRelativeToNowAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrain>("Expires_After_AbsoluteExpirationRelativeToNow");
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMilliseconds(100),
        SlidingExpiration: default
    );
    var value = await grain.GetOrCreateAsync(CancellationToken.None, options);
    value.IsOk.Should().BeTrue();
    value.Value.Should().Be("volatile in cluster cache");

    // Wait for expiration (use a little buffer)
    await Task.Delay(TimeSpan.FromMilliseconds(200));

    var fetchedAfter = await grain.GetAsync(CancellationToken.None);
    fetchedAfter.IsOk.Should().BeFalse();
    fetchedAfter.Value.Should().BeNull();
  }
}

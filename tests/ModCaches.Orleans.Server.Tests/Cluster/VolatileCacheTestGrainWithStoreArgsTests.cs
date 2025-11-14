using AwesomeAssertions;
using ModCaches.Orleans.Abstractions.Cluster;

namespace ModCaches.Orleans.Server.Tests.Cluster;

[Collection(ClusterCollection.Name)]
public class VolatileCacheTestGrainWithStoreArgsTests
{
  private readonly ClusterFixture _fixture;

  public VolatileCacheTestGrainWithStoreArgsTests(ClusterFixture fixture)
  {
    _fixture = fixture;
  }

  [Fact]
  public async Task SetAndWriteAsync_ReturnsWrittenValueAsync()
  {
    var defaultData = "default-data";
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrainWithStoreArgs>("SetAndWriteAsync_ReturnsWrittenValue");
    var result = await grain.SetAndWriteAsync(defaultData, CancellationToken.None);
    result.Should().Be("write-through " + defaultData);
  }

  [Fact]
  public async Task GetOrCreateAsync_ReturnsGeneratedValue_WhenNotSetAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrainWithStoreArgs>("GetOrCreate_ReturnsGeneratedValue");
    var result = await grain.GetOrCreateAsync(2, CancellationToken.None);
    result.Should().Be("volatile in cluster cache 2");
  }

  [Fact]
  public async Task CreateAsync_Then_GetOrCreateAsync_ReturnsValueAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrainWithStoreArgs>("Create_Then_GetOrCreate");
    // Force creation
    var created = await grain.CreateAsync(2, CancellationToken.None);
    created.Should().Be("volatile in cluster cache 2");

    // Then ensure subsequent GetOrCreate returns value (cached)
    var fetched = await grain.GetOrCreateAsync(3, CancellationToken.None);
    fetched.Should().Be("volatile in cluster cache 2");
  }

  [Fact]
  public async Task SetAsync_Then_TryGetAsync_ReturnsValueAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrainWithStoreArgs>("Set_Then_TryGet");
    await grain.SetAsync("custom-value", CancellationToken.None, null);

    var (found, value) = await grain.TryGetAsync(CancellationToken.None);
    found.Should().BeTrue();
    value.Should().Be("custom-value");
  }

  [Fact]
  public async Task RemoveAsync_RemovesValueSoTryGetReturnsNullAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrainWithStoreArgs>("Remove_RemovesValue");
    await grain.SetAsync("to-be-removed", CancellationToken.None, null);

    // ensure set
    var (foundBefore, _) = await grain.TryGetAsync(CancellationToken.None);
    foundBefore.Should().BeTrue();

    // remove and verify
    await grain.RemoveAsync(CancellationToken.None);
    var (foundAfter, valueAfter) = await grain.TryGetAsync(CancellationToken.None);
    foundAfter.Should().BeFalse();
    valueAfter.Should().BeNull();
  }

  [Fact]
  public async Task RefreshAsync_ExtendsSlidingLifetime_WhenNotExpiredAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrainWithStoreArgs>("Refresh_Extends");
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
    refreshed.Should().BeTrue();

    // Wait again beyond original remaining time but within refreshed lifetime
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    var (found, value) = await grain.TryGetAsync(CancellationToken.None);
    found.Should().BeTrue();
    value.Should().Be("refresh-test");
  }

  [Fact]
  public async Task RefreshAsync_DoesNotExtendAbsoluteLifetime_WhenNotExpiredAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrainWithStoreArgs>("Refresh_DoesNotExtend");
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
    refreshed.Should().BeTrue();

    // Wait again beyond original remaining time but within refreshed lifetime
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    var (foundAfter, valueAfter) = await grain.TryGetAsync(CancellationToken.None);
    foundAfter.Should().BeFalse();
    valueAfter.Should().BeNull();
  }

  [Fact]
  public async Task PeekAsync_DoesNotExtendSlidingLifetimeAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrainWithStoreArgs>("Peek_DoesNotExtend");
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
    var (found, value) = await grain.TryPeekAsync(CancellationToken.None);
    found.Should().BeTrue();
    value.Should().Be("peek-test");

    // Wait again beyond original remaining time but within refreshed lifetime
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    var (foundAfter, valueAfter) = await grain.TryGetAsync(CancellationToken.None);
    foundAfter.Should().BeFalse();
    valueAfter.Should().BeNull();
  }

  [Fact]
  public async Task CachedValue_Expires_AfterAbsoluteExpirationRelativeToNowAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileCacheTestGrainWithStoreArgs>("Expires_After_AbsoluteExpirationRelativeToNow");
    var options = new CacheGrainEntryOptions
    (
        AbsoluteExpiration: default,
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMilliseconds(100),
        SlidingExpiration: default
    );
    var value = await grain.GetOrCreateAsync(4, CancellationToken.None, options);
    value.Should().Be("volatile in cluster cache 4");

    // Wait for expiration (use a little buffer)
    await Task.Delay(TimeSpan.FromMilliseconds(200));

    var (found, afterValue) = await grain.TryGetAsync(CancellationToken.None);
    found.Should().BeFalse();
    afterValue.Should().BeNull();
  }
}

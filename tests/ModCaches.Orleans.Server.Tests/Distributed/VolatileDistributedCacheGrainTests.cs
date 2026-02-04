using System.Collections.Immutable;
using AwesomeAssertions;
using ModCaches.Orleans.Abstractions.Distributed;

namespace ModCaches.Orleans.Server.Tests.Distributed;

[Collection(ClusterCollection.Name)]
public class VolatileDistributedCacheGrainTests
{
  private readonly ClusterFixture _fixture;

  public VolatileDistributedCacheGrainTests(ClusterFixture fixture)
  {
    _fixture = fixture;
  }

  [Fact]
  public async Task GetAsync_ReturnsNull_WhenNotSetAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileDistributedCacheGrain>("GetAsync_ReturnsNull_WhenNotSet");
    var result = await grain.GetAsync(TestContext.Current.CancellationToken);
    result.Should().BeNull();
  }

  [Fact]
  public async Task SetAsync_Then_GetAsync_ReturnsValueAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileDistributedCacheGrain>("SetThenGet");
    var data = ImmutableArray.Create<byte>(1, 2, 3, 4);
    var options = new Abstractions.Common.CacheEntryOptions(
      AbsoluteExpiration: null,
      AbsoluteExpirationRelativeToNow: TimeSpan.FromMinutes(5),
      SlidingExpiration: null);

    await grain.SetAsync(data, options, TestContext.Current.CancellationToken);

    var fetched = await grain.GetAsync(TestContext.Current.CancellationToken);
    fetched.Should().NotBeNull();
    fetched.Value.Should().Equal(data);
  }

  [Fact]
  public async Task RemoveAsync_RemovesValueSoGetReturnsNullAsync()
  {
    var key = "Remove_RemovesValue";
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileDistributedCacheGrain>(key);
    var data = ImmutableArray.Create<byte>(9, 8, 7);

    var options = new Abstractions.Common.CacheEntryOptions(
      AbsoluteExpiration: null,
      AbsoluteExpirationRelativeToNow: TimeSpan.FromMinutes(1),
      SlidingExpiration: null);

    await grain.SetAsync(data, options, TestContext.Current.CancellationToken);

    // ensure it was set
    var fetched = await grain.GetAsync(TestContext.Current.CancellationToken);
    fetched.Should().NotBeNull();

    // remove and verify
    await grain.RemoveAsync(TestContext.Current.CancellationToken);
    var afterRemove = await grain.GetAsync(TestContext.Current.CancellationToken);
    afterRemove.Should().BeNull();
  }

  [Fact]
  public async Task RefreshAsync_ExtendsLifetime_WhenNotExpiredAsync()
  {
    var key = "Refresh_Extends";
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileDistributedCacheGrain>(key);
    var data = ImmutableArray.Create<byte>(5, 6, 7);

    var options = new Abstractions.Common.CacheEntryOptions(
      AbsoluteExpiration: null,
      AbsoluteExpirationRelativeToNow: TimeSpan.FromMinutes(5),
      SlidingExpiration: null);

    await grain.SetAsync(data, options, TestContext.Current.CancellationToken);

    // call refresh; since not expired it should remain available
    await grain.RefreshAsync(TestContext.Current.CancellationToken);

    var fetched = await grain.GetAsync(TestContext.Current.CancellationToken);
    fetched.Should().NotBeNull();
    Assert.Equal(data, fetched);
  }

  [Fact]
  public async Task RefreshAsync_Removes_WhenExpiredAsync()
  {
    var key = "Refresh_Removes_WhenExpired";
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileDistributedCacheGrain>(key);
    var data = ImmutableArray.Create<byte>(11, 22, 33);

    var options = new Abstractions.Common.CacheEntryOptions(
      AbsoluteExpiration: null,
      AbsoluteExpirationRelativeToNow: TimeSpan.FromMilliseconds(50),
      SlidingExpiration: null);

    await grain.SetAsync(data, options, TestContext.Current.CancellationToken);

    // Wait for the entry to expire
    await Task.Delay(150, TestContext.Current.CancellationToken);

    // Refresh should detect expiration and remove the entry
    await grain.RefreshAsync(TestContext.Current.CancellationToken);

    var fetched = await grain.GetAsync(TestContext.Current.CancellationToken);
    fetched.Should().BeNull();
  }
}

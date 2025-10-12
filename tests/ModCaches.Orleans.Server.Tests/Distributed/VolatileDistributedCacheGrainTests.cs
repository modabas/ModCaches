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
    var result = await grain.GetAsync(CancellationToken.None);
    result.Should().BeNull();
  }

  [Fact]
  public async Task SetAsync_Then_GetAsync_ReturnsValueAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileDistributedCacheGrain>("SetThenGet");
    var data = ImmutableArray.Create<byte>(1, 2, 3, 4);
    var options = new ModCaches.Orleans.Abstractions.Common.CacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    await grain.SetAsync(data, options, CancellationToken.None);

    var fetched = await grain.GetAsync(CancellationToken.None);
    fetched.Should().NotBeNull();
    fetched.Value.Should().Equal(data);
  }

  [Fact]
  public async Task RemoveAsync_RemovesValueSoGetReturnsNullAsync()
  {
    var key = "Remove_RemovesValue";
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileDistributedCacheGrain>(key);
    var data = ImmutableArray.Create<byte>(9, 8, 7);

    var options = new ModCaches.Orleans.Abstractions.Common.CacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
    };

    await grain.SetAsync(data, options, CancellationToken.None);

    // ensure it was set
    var fetched = await grain.GetAsync(CancellationToken.None);
    fetched.Should().NotBeNull();

    // remove and verify
    await grain.RemoveAsync(CancellationToken.None);
    var afterRemove = await grain.GetAsync(CancellationToken.None);
    afterRemove.Should().BeNull();
  }

  [Fact]
  public async Task RefreshAsync_ExtendsLifetime_WhenNotExpiredAsync()
  {
    var key = "Refresh_Extends";
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileDistributedCacheGrain>(key);
    var data = ImmutableArray.Create<byte>(5, 6, 7);

    var options = new ModCaches.Orleans.Abstractions.Common.CacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
    };

    await grain.SetAsync(data, options, CancellationToken.None);

    // call refresh; since not expired it should remain available
    await grain.RefreshAsync(CancellationToken.None);

    var fetched = await grain.GetAsync(CancellationToken.None);
    fetched.Should().NotBeNull();
    Assert.Equal(data, fetched);
  }

  [Fact]
  public async Task RefreshAsync_Removes_WhenExpiredAsync()
  {
    var key = "Refresh_Removes_WhenExpired";
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IVolatileDistributedCacheGrain>(key);
    var data = ImmutableArray.Create<byte>(11, 22, 33);

    var options = new ModCaches.Orleans.Abstractions.Common.CacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(50)
    };

    await grain.SetAsync(data, options, CancellationToken.None);

    // Wait for the entry to expire
    await Task.Delay(150);

    // Refresh should detect expiration and remove the entry
    await grain.RefreshAsync(CancellationToken.None);

    var fetched = await grain.GetAsync(CancellationToken.None);
    fetched.Should().BeNull();
  }
}

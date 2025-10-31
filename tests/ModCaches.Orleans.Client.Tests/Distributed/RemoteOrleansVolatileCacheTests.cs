using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Distributed;
using ModCaches.Orleans.Abstractions.Common;
using ModCaches.Orleans.Abstractions.Distributed;
using ModCaches.Orleans.Client.Distributed;
using NSubstitute;

namespace ModCaches.Orleans.Client.Tests.Distributed;

public class RemoteOrleansVolatileCacheTests
{
  private const string Key = "test-key";

  [Fact]
  public async Task Get_ReturnsBytesAsync()
  {
    // Arrange
    var expected = new byte[] { 1, 2, 3 };
    var grain = Substitute.For<IVolatileDistributedCacheGrain>();
    grain.GetAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<ImmutableArray<byte>?>(ImmutableArray.Create(expected)));

    var cluster = Substitute.For<IClusterClient>();
    cluster.GetGrain<IVolatileDistributedCacheGrain>(Arg.Any<string>()).Returns(grain);

    var cache = new RemoteOrleansVolatileCache(cluster);

    // Act
    var actual = cache.Get(Key);

    // Assert
    actual.Should().NotBeNull();
    actual.Should().Equal(expected);
    await grain.Received(1).GetAsync(Arg.Any<CancellationToken>());
    cluster.Received(1).GetGrain<IVolatileDistributedCacheGrain>(Key);
  }

  [Fact]
  public async Task GetAsync_ReturnsBytesAsync()
  {
    // Arrange
    var expected = new byte[] { 9, 8, 7 };
    var grain = Substitute.For<IVolatileDistributedCacheGrain>();
    grain.GetAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<ImmutableArray<byte>?>(ImmutableArray.Create(expected)));

    var cluster = Substitute.For<IClusterClient>();
    cluster.GetGrain<IVolatileDistributedCacheGrain>(Arg.Any<string>()).Returns(grain);

    var cache = new RemoteOrleansVolatileCache(cluster);

    // Act
    var actual = await cache.GetAsync(Key, CancellationToken.None);

    // Assert
    actual.Should().NotBeNull();
    actual.Should().Equal(expected);
    await grain.Received(1).GetAsync(Arg.Any<CancellationToken>());
    cluster.Received(1).GetGrain<IVolatileDistributedCacheGrain>(Key);
  }

  [Fact]
  public async Task Refresh_CallsGrainRefreshAsync()
  {
    // Arrange
    var grain = Substitute.For<IVolatileDistributedCacheGrain>();
    grain.RefreshAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<bool>(true));

    var cluster = Substitute.For<IClusterClient>();
    cluster.GetGrain<IVolatileDistributedCacheGrain>(Arg.Any<string>()).Returns(grain);

    var cache = new RemoteOrleansVolatileCache(cluster);

    // Act
    cache.Refresh(Key);

    // Assert
    await grain.Received(1).RefreshAsync(Arg.Any<CancellationToken>());
    cluster.Received(1).GetGrain<IVolatileDistributedCacheGrain>(Key);
  }

  [Fact]
  public async Task RefreshAsync_CallsGrainRefreshAsync()
  {
    // Arrange
    var grain = Substitute.For<IVolatileDistributedCacheGrain>();
    grain.RefreshAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<bool>(true));

    var cluster = Substitute.For<IClusterClient>();
    cluster.GetGrain<IVolatileDistributedCacheGrain>(Arg.Any<string>()).Returns(grain);

    var cache = new RemoteOrleansVolatileCache(cluster);

    // Act
    await cache.RefreshAsync(Key, CancellationToken.None);

    // Assert
    await grain.Received(1).RefreshAsync(Arg.Any<CancellationToken>());
    cluster.Received(1).GetGrain<IVolatileDistributedCacheGrain>(Key);
  }

  [Fact]
  public async Task Remove_CallsGrainRemoveAsync()
  {
    // Arrange
    var grain = Substitute.For<IVolatileDistributedCacheGrain>();
    grain.RemoveAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

    var cluster = Substitute.For<IClusterClient>();
    cluster.GetGrain<IVolatileDistributedCacheGrain>(Arg.Any<string>()).Returns(grain);

    var cache = new RemoteOrleansVolatileCache(cluster);

    // Act
    cache.Remove(Key);

    // Assert
    await grain.Received(1).RemoveAsync(Arg.Any<CancellationToken>());
    cluster.Received(1).GetGrain<IVolatileDistributedCacheGrain>(Key);
  }

  [Fact]
  public async Task RemoveAsync_CallsGrainRemoveAsync()
  {
    // Arrange
    var grain = Substitute.For<IVolatileDistributedCacheGrain>();
    grain.RemoveAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

    var cluster = Substitute.For<IClusterClient>();
    cluster.GetGrain<IVolatileDistributedCacheGrain>(Arg.Any<string>()).Returns(grain);

    var cache = new RemoteOrleansVolatileCache(cluster);

    // Act
    await cache.RemoveAsync(Key, CancellationToken.None);

    // Assert
    await grain.Received(1).RemoveAsync(Arg.Any<CancellationToken>());
    cluster.Received(1).GetGrain<IVolatileDistributedCacheGrain>(Key);
  }

  [Fact]
  public async Task Set_CallsGrainSetAsync()
  {
    // Arrange
    var value = new byte[] { 5, 6 };
    var options = new DistributedCacheEntryOptions
    {
      AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(1),
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
      SlidingExpiration = TimeSpan.FromSeconds(30)
    };

    var grain = Substitute.For<IVolatileDistributedCacheGrain>();
    grain.SetAsync(Arg.Any<ImmutableArray<byte>>(), Arg.Any<CacheEntryOptions>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

    var cluster = Substitute.For<IClusterClient>();
    cluster.GetGrain<IVolatileDistributedCacheGrain>(Arg.Any<string>()).Returns(grain);

    var cache = new RemoteOrleansVolatileCache(cluster);

    // Act
    cache.Set(Key, value, options);

    // Assert
    await grain.Received(1).SetAsync(
      Arg.Is<ImmutableArray<byte>>(a => a.ToArray().SequenceEqual(value)),
      Arg.Is<CacheEntryOptions>(o =>
        o.AbsoluteExpiration.HasValue &&
        o.AbsoluteExpirationRelativeToNow.HasValue &&
        o.SlidingExpiration.HasValue),
      Arg.Any<CancellationToken>());

    cluster.Received(1).GetGrain<IVolatileDistributedCacheGrain>(Key);
  }

  [Fact]
  public async Task SetAsync_CallsGrainSetAsync()
  {
    // Arrange
    var value = new byte[] { 10, 11, 12 };
    var options = new DistributedCacheEntryOptions
    {
      AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(2),
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
      SlidingExpiration = TimeSpan.FromSeconds(45)
    };

    var grain = Substitute.For<IVolatileDistributedCacheGrain>();
    grain.SetAsync(Arg.Any<ImmutableArray<byte>>(), Arg.Any<CacheEntryOptions>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

    var cluster = Substitute.For<IClusterClient>();
    cluster.GetGrain<IVolatileDistributedCacheGrain>(Arg.Any<string>()).Returns(grain);

    var cache = new RemoteOrleansVolatileCache(cluster);

    // Act
    await cache.SetAsync(Key, value, options, CancellationToken.None);

    // Assert
    await grain.Received(1).SetAsync(
      Arg.Is<ImmutableArray<byte>>(a => a.ToArray().SequenceEqual(value)),
      Arg.Is<CacheEntryOptions>(o =>
        o.AbsoluteExpiration.HasValue &&
        o.AbsoluteExpirationRelativeToNow.HasValue &&
        o.SlidingExpiration.HasValue),
      Arg.Any<CancellationToken>());

    cluster.Received(1).GetGrain<IVolatileDistributedCacheGrain>(Key);
  }
}

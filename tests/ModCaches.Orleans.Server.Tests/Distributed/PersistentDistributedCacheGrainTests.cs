using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModCaches.Orleans.Abstractions.Common;
using ModCaches.Orleans.Abstractions.Distributed;
using ModCaches.Orleans.Server.Distributed;
using ModCaches.Orleans.Server.InCluster;
using ModCaches.Orleans.Server.Tests.InCluster;
using Orleans.Providers;
using Orleans.Storage;

namespace ModCaches.Orleans.Server.Tests.Distributed;

[Collection(ClusterCollection.Name)]
public class PersistentDistributedCacheGrainTests
{
  private readonly ClusterFixture _fixture;

  private GrainId GetGrainId(string key)
  {
    return OrleansHelpers.GetGrainIdFactory(_fixture).CreateGrainId<IPersistentDistributedCacheGrain>(key);
  }

  private async Task<GrainState<DistributedCacheState>> GetStateAsync(GrainId grainId)
  {
    GrainState<DistributedCacheState> state = new();
    await OrleansHelpers.GetDefaultGrainStorage(_fixture).ReadStateAsync(nameof(PersistentDistributedCacheGrain), grainId, state);
    return state;
  }

  private async Task<GrainState<DistributedCacheState>> SetStateAsync(GrainId grainId, DistributedCacheState cacheState)
  {
    GrainState<DistributedCacheState> state = new(cacheState);
    await OrleansHelpers.GetDefaultGrainStorage(_fixture).WriteStateAsync(nameof(PersistentDistributedCacheGrain), grainId, state);
    return state;
  }

  public PersistentDistributedCacheGrainTests(ClusterFixture fixture)
  {
    _fixture = fixture;
  }

  [Fact]
  public async Task GetAsync_ReturnsNull_WhenNotSetAsync()
  {
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentDistributedCacheGrain>("GetAsync_ReturnsNull_WhenNotSet");
    var result = await grain.GetAsync(CancellationToken.None);
    result.Should().BeNull();
  }

  [Fact]
  public async Task SetAsync_Then_GetAsync_ReturnsValueAsync()
  {
    var grainId = GetGrainId("SetThenGet");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentDistributedCacheGrain>(grainId);
    var data = ImmutableArray.Create<byte>(1, 2, 3, 4);
    var options = new CacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    await grain.SetAsync(data, options, CancellationToken.None);

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Should().Equal(data);

    var fetched = await grain.GetAsync(CancellationToken.None);
    fetched.Should().NotBeNull();
    fetched.Value.Should().Equal(data);
  }

  [Fact]
  public async Task RemoveAsync_RemovesValueSoGetReturnsNullAsync()
  {
    var grainId = GetGrainId("Remove_RemovesValue");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentDistributedCacheGrain>(grainId);
    var data = ImmutableArray.Create<byte>(9, 8, 7);

    var options = new CacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
    };

    await grain.SetAsync(data, options, CancellationToken.None);

    // ensure it was set
    var fetched = await grain.GetAsync(CancellationToken.None);
    fetched.Should().NotBeNull();
    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();

    // remove and verify
    await grain.RemoveAsync(CancellationToken.None);
    var afterRemove = await grain.GetAsync(CancellationToken.None);
    afterRemove.Should().BeNull();
    var stateAfterRemove = await GetStateAsync(grainId);
    stateAfterRemove.Should().NotBeNull();
    stateAfterRemove.RecordExists.Should().BeFalse();
    stateAfterRemove.State.LastAccessed.Should().Be(DateTimeOffset.MinValue);
  }

  [Fact]
  public async Task RefreshAsync_ExtendsLifetime_WhenNotExpiredAsync()
  {
    var grainId = GetGrainId("Refresh_Extends");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentDistributedCacheGrain>(grainId);
    var data = ImmutableArray.Create<byte>(5, 6, 7);

    var options = new CacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
    };

    await grain.SetAsync(data, options, CancellationToken.None);

    // call refresh; since not expired it should remain available
    await grain.RefreshAsync(CancellationToken.None);

    var fetched = await grain.GetAsync(CancellationToken.None);
    fetched.Should().NotBeNull();
    fetched.Value.Should().Equal(data);

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Should().Equal(data);
  }

  [Fact]
  public async Task RefreshAsync_Removes_WhenExpiredAsync()
  {
    var grainId = GetGrainId("Refresh_Removes_WhenExpired");
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentDistributedCacheGrain>(grainId);
    var data = ImmutableArray.Create<byte>(11, 22, 33);

    var options = new CacheEntryOptions
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

    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.RecordExists.Should().BeFalse();
    state.State.LastAccessed.Should().Be(DateTimeOffset.MinValue);
  }

  [Fact]
  public async Task OnActivate_Removes_StaleStateAsync()
  {
    var grainId = GetGrainId("OnActivate_Removes_StaleState");
    var data = ImmutableArray.Create<byte>(11, 22, 33);
    var cacheState = new DistributedCacheState
    {
      Value = data,
      AbsoluteExpiration = DateTimeOffset.UtcNow.AddMilliseconds(-500),
      LastAccessed = DateTimeOffset.UtcNow.AddMilliseconds(-1000)
    };
    await SetStateAsync(grainId, cacheState);
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentDistributedCacheGrain>(grainId);

    var result = await grain.GetAsync(CancellationToken.None);
    result.Should().BeNull();
    var stateAfterRemove = await GetStateAsync(grainId);
    stateAfterRemove.Should().NotBeNull();
    stateAfterRemove.RecordExists.Should().BeFalse();
    stateAfterRemove.State.LastAccessed.Should().Be(DateTimeOffset.MinValue);
  }

  [Fact]
  public async Task OnActivate_Keeps_ValidStateAsync()
  {
    var grainId = GetGrainId("OnActivate_Keeps_ValidState");
    var data = ImmutableArray.Create<byte>(12, 23, 34);
    var cacheState = new DistributedCacheState
    {
      Value = data,
      AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(3600),
      LastAccessed = DateTimeOffset.UtcNow.AddMilliseconds(-1000)
    };
    await SetStateAsync(grainId, cacheState);
    var grain = _fixture.Cluster.GrainFactory.GetGrain<IPersistentDistributedCacheGrain>(grainId);

    var fetched = await grain.GetAsync(CancellationToken.None);
    fetched.Should().NotBeNull();
    fetched.Value.Should().Equal(data);
    var state = await GetStateAsync(grainId);
    state.Should().NotBeNull();
    state.State.Value.Should().Equal(data);
  }
}

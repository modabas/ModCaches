using System.Buffers;
using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Distributed;
using Orleans;

namespace ModCaches.OrleansCaches.Distributed;

public class CoHostedOrleansCache : IDistributedCache
{
  private readonly IGrainFactory _grainFactory;

  public CoHostedOrleansCache(IGrainFactory grainFactory)
  {
    _grainFactory = grainFactory;
  }

  public byte[]? Get(string key)
  {
    throw new NotImplementedException();
  }

  public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
  {
    return (await _grainFactory.GetGrain<IDistributedCacheGrain>(key).GetAsync(token))?.ToArray();
  }

  public void Refresh(string key)
  {
    throw new NotImplementedException();
  }

  public async Task RefreshAsync(string key, CancellationToken token = default)
  {
    await _grainFactory.GetGrain<IDistributedCacheGrain>(key).RefreshAsync(token);
  }

  public void Remove(string key)
  {
    throw new NotImplementedException();
  }

  public async Task RemoveAsync(string key, CancellationToken token = default)
  {
    await _grainFactory.GetGrain<IDistributedCacheGrain>(key).RemoveAsync(token);
  }

  public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
  {
    throw new NotImplementedException();
  }

  public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
  {
    await _grainFactory.GetGrain<IDistributedCacheGrain>(key).SetAsync(value.ToImmutableArray(), options.ToOrleansCacheEntryOptions(), token);
  }
}

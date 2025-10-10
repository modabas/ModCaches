using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Distributed;
using ModCaches.Orleans.Abstractions.Distributed;

namespace ModCaches.Orleans.Client.Distributed;

public class RemoteOrleansVolatileCache : IDistributedCache
{
  private readonly IClusterClient _clusterClient;

  public RemoteOrleansVolatileCache(IClusterClient clusterClient)
  {
    _clusterClient = clusterClient;
  }

  public byte[]? Get(string key)
  {
    return GetAsync(key).GetAwaiter().GetResult();
  }

  public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
  {
    return (await _clusterClient.GetGrain<IVolatileDistributedCacheGrain>(key).GetAsync(token))?.ToArray();
  }

  public void Refresh(string key)
  {
    RefreshAsync(key).GetAwaiter().GetResult();
  }

  public async Task RefreshAsync(string key, CancellationToken token = default)
  {
    await _clusterClient.GetGrain<IVolatileDistributedCacheGrain>(key).RefreshAsync(token);
  }

  public void Remove(string key)
  {
    RemoveAsync(key).GetAwaiter().GetResult();
  }

  public async Task RemoveAsync(string key, CancellationToken token = default)
  {
    await _clusterClient.GetGrain<IVolatileDistributedCacheGrain>(key).RemoveAsync(token);
  }

  public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
  {
    SetAsync(key, value, options).GetAwaiter().GetResult();
  }

  public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
  {
    await _clusterClient.GetGrain<IVolatileDistributedCacheGrain>(key).SetAsync(value.ToImmutableArray(), options.ToOrleansCacheEntryOptions(), token);
  }
}

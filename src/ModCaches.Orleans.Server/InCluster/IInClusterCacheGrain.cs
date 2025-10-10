namespace ModCaches.Orleans.Server.InCluster;

public interface IInClusterCacheGrain<TValue, TCreateArgs> : IBaseInClusterCacheGrain<TValue>
  where TValue : notnull
  where TCreateArgs : notnull
{
  Task<TValue> GetOrCreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null);

  Task<TValue> CreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null);
}

public interface IInClusterCacheGrain<TValue> : IBaseInClusterCacheGrain<TValue>
  where TValue : notnull
{
  Task<TValue> GetOrCreateAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null);

  Task<TValue> CreateAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null);
}

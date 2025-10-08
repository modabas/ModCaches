using Orleans;

namespace ModCaches.OrleansCaches.InCluster;

public interface IBaseInClusterCacheGrain<TValue> : IGrainWithStringKey
  where TValue : notnull
{
  Task<bool> RefreshAsync(CancellationToken ct);

  Task RemoveAsync(CancellationToken ct);

  Task<(bool, TValue?)> TryGetAsync(CancellationToken ct);

  Task<(bool, TValue?)> TryPeekAsync(CancellationToken ct);

  Task SetAsync(
    TValue value,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null);
}

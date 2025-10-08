using Microsoft.Extensions.Caching.Distributed;

namespace ModCaches.ExtendedDistributedCache;

// Provides an extended interface for distributed caching similar to HybridCache.
// It allows for asynchronous creation of cache entries using a factory function.
public interface IExtendedDistributedCache
{
  IDistributedCache DistributedCache { get; }

  Task<T> GetOrCreateAsync<T>(
    string key,
    Func<CancellationToken, Task<T>> factory,
    CancellationToken ct,
    DistributedCacheEntryOptions? options = null);

  Task SetAsync<T>(
    string key,
    T value,
    CancellationToken ct,
    DistributedCacheEntryOptions? options = null);

  Task<(bool, T?)> TryGetValueAsync<T>(string key, CancellationToken ct);
}

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using ModCaches.ExtendedDistributedCache.Lru;

namespace ModCaches.ExtendedDistributedCache;

// Provides an extended interface for distributed caching similar to HybridCache.
// It allows for asynchronous creation of cache entries using a factory function.
// This implementation uses semaphores to ensure that only one thread can create an entry with same cache key, for cache stampede protection.
// It also supports a maximum number of locks to prevent excessive memory usage.
// ConcurrentLruCache implementation is from Microsoft Orleans project.
// Serialization and deserialization of cache entries is handled by ICacheSerializer.
// The cache entry options can be customized through ExtendedDistributedCacheOptions.
internal class DefaultExtendedDistributedCache : IExtendedDistributedCache
{
  private readonly IDistributedCache _cache;
  private readonly IOptions<ExtendedDistributedCacheOptions> _options;
  private readonly IDistributedCacheSerializer _serializer;
  private readonly ConcurrentLruCache<string, SemaphoreSlim> _locks;
  private static Func<string, SemaphoreSlim> _semaphoreFactory = _ => new SemaphoreSlim(1);

  public IDistributedCache DistributedCache => _cache;

  public DefaultExtendedDistributedCache(
    IDistributedCache cache,
    IOptions<ExtendedDistributedCacheOptions> options,
    IDistributedCacheSerializer serializer)
  {
    _cache = cache;
    _options = options;
    _serializer = serializer;
    _locks = _options.Value.MaxLocks > 0
      ? new(_options.Value.MaxLocks)
      : new(ExtendedDistributedCacheOptions.DefaultMaxLocks); // Default capacity if not set
  }

  public Task<T> GetOrCreateAsync<T>(
    string key,
    Func<CancellationToken, Task<T>> factory,
    CancellationToken ct,
    DistributedCacheEntryOptions? options = null)
  {
    //allows GetOrCreateAsync<T> and GetOrCreateAsync<TState, T> to share an implementation.
    Func<Func<CancellationToken, Task<T>>, CancellationToken, Task<T>> wrappedFactoryCallback = (callback, ct) => callback(ct);
    return GetOrCreateAsync(key, factory, wrappedFactoryCallback, ct, options);
  }

  public async Task<T> GetOrCreateAsync<TState, T>(string key, TState state, Func<TState, CancellationToken, Task<T>> factory, CancellationToken ct, DistributedCacheEntryOptions? options = null)
  {
    //Read the cache first
    var bytes = await _cache.GetAsync(key, ct);
    if (bytes is null)
    {
      // If the cache entry does not exist, we need to create it.
      // Use a semaphore to ensure that only one thread can create the entry.
      var keyLock = _locks.GetOrAdd(key, _semaphoreFactory);
      await keyLock.WaitAsync(ct);
      try
      {
        // Double-check if the cache entry was created while waiting for the lock.
        bytes = await _cache.GetAsync(key, ct);
        if (bytes is null)
        {
          var value = await factory(state, ct);
          await SetAsync(key, value, ct, options);
          return value;
        }
      }
      finally
      {
        keyLock.Release();
      }
    }
    return await _serializer.DeserializeAsync<T>(bytes, ct) ??
      throw new InvalidOperationException("Deserialized value is null.");
  }

  public async Task SetAsync<T>(
    string key,
    T value,
    CancellationToken ct,
    DistributedCacheEntryOptions? options = null)
  {
    var bytes = await _serializer.SerializeAsync(value, ct);
    var cacheEntryOptions = GetCacheEntryOptions(options);
    await _cache.SetAsync(key, bytes.ToArray(), cacheEntryOptions, ct);
  }

  private DistributedCacheEntryOptions GetCacheEntryOptions(DistributedCacheEntryOptions? options)
  {
    return options ?? new DistributedCacheEntryOptions()
    {
      AbsoluteExpiration = _options.Value.AbsoluteExpiration,
      AbsoluteExpirationRelativeToNow = _options.Value.AbsoluteExpirationRelativeToNow,
      SlidingExpiration = _options.Value.SlidingExpiration
    };
  }

  public async Task<(bool, T?)> TryGetValueAsync<T>(string key, CancellationToken ct)
  {
    var bytes = await _cache.GetAsync(key, ct);
    if (bytes is null)
    {
      return (false, default);
    }
    var value = await _serializer.DeserializeAsync<T>(bytes, ct) ??
      throw new InvalidOperationException("Deserialized value is null.");
    return (true, value);
  }
}

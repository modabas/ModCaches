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
internal sealed class DefaultExtendedDistributedCache : IExtendedDistributedCache
{
  private readonly IDistributedCache _cache;
  private readonly IOptions<ExtendedDistributedCacheOptions> _options;
  private readonly IDistributedCacheSerializer _serializer;
  private readonly ConcurrentLruCache<string, SemaphoreSlim> _locks;

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
    return GetOrCreateAsync(key, factory, WrapFactoryCallback, ct, options);

    //allows GetOrCreateAsync<T> and GetOrCreateAsync<TState, T> to share an implementation.
    static Task<T> WrapFactoryCallback(Func<CancellationToken, Task<T>> callback, CancellationToken ct) => callback(ct);
  }

  public async Task<T> GetOrCreateAsync<TState, T>(string key, TState state, Func<TState, CancellationToken, Task<T>> factory, CancellationToken ct, DistributedCacheEntryOptions? options = null)
  {
    //Read the cache first
    var bytes = await _cache.GetAsync(key, ct).ConfigureAwait(false);
    if (bytes is null)
    {
      // If the cache entry does not exist, we need to create it.
      // Use a semaphore to ensure that only one thread can create the entry.
      var keyLock = _locks.GetOrAdd(key, CreateLockSemaphore);
      await keyLock.WaitAsync(ct).ConfigureAwait(false);
      try
      {
        // Double-check if the cache entry was created while waiting for the lock.
        bytes = await _cache.GetAsync(key, ct).ConfigureAwait(false);
        if (bytes is null)
        {
          var value = await factory(state, ct).ConfigureAwait(false);
          await SetAsync(key, value, ct, options).ConfigureAwait(false);
          return value;
        }
      }
      finally
      {
        keyLock.Release();
      }
    }
    return await _serializer.DeserializeAsync<T>(bytes, ct).ConfigureAwait(false) ??
      throw new InvalidOperationException("Deserialized value is null.");
  }

  private static SemaphoreSlim CreateLockSemaphore(string key) => new(1);

  public async Task SetAsync<T>(
    string key,
    T value,
    CancellationToken ct,
    DistributedCacheEntryOptions? options = null)
  {
    var bytes = await _serializer.SerializeAsync(value, ct).ConfigureAwait(false);
    var cacheEntryOptions = GetCacheEntryOptions(options);
    await _cache.SetAsync(key, bytes.ToArray(), cacheEntryOptions, ct).ConfigureAwait(false);
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

  public async Task<(bool IsOk, T? Value)> TryGetValueAsync<T>(string key, CancellationToken ct)
  {
    var bytes = await _cache.GetAsync(key, ct).ConfigureAwait(false);
    if (bytes is null)
    {
      return (IsOk: false, Value: default);
    }
    var value = await _serializer.DeserializeAsync<T>(bytes, ct).ConfigureAwait(false) ??
      throw new InvalidOperationException("Deserialized value is null.");
    return (IsOk: true, Value: value);
  }
}

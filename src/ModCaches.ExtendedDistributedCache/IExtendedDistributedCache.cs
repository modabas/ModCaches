using Microsoft.Extensions.Caching.Distributed;

namespace ModCaches.ExtendedDistributedCache;

// Provides an extended interface for distributed caching similar to HybridCache.
// It allows for asynchronous creation of cache entries using a factory function.
public interface IExtendedDistributedCache
{
  /// <summary>
  /// Underlying IDistributedCache
  /// </summary>
  IDistributedCache DistributedCache { get; }

  /// <summary>
  /// Asynchronously gets the value associated with the key if it exists, or generates a new entry using the provided key and a value from the given factory if the key is not found.
  /// </summary>
  /// <typeparam name="T">The type of the data being considered.</typeparam>
  /// <param name="key">The key of the entry to look for or create.</param>
  /// <param name="factory">Provides the underlying data service if the data is not available in the cache.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">Additional options for this cache entry.</param>
  /// <returns>The data, either from cache or the underlying data service.</returns>
  Task<T> GetOrCreateAsync<T>(
    string key,
    Func<CancellationToken, Task<T>> factory,
    CancellationToken ct,
    DistributedCacheEntryOptions? options = null);

  /// <summary>
  /// Asynchronously gets the value associated with the key if it exists, or generates a new entry using the provided key and a value from the given factory if the key is not found.
  /// </summary>
  /// <typeparam name="TState">The type of additional state required by <paramref name="factory"/>.</typeparam>
  /// <typeparam name="T">The type of the data being considered.</typeparam>
  /// <param name="key">The key of the entry to look for or create.</param>
  /// <param name="factory">Provides the underlying data service if the data is not available in the cache.</param>
  /// <param name="state">The state required for <paramref name="factory"/>.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">Additional options for this cache entry.</param>
  /// <returns>The data, either from cache or the underlying data service.</returns>
  Task<T> GetOrCreateAsync<TState, T>(
    string key,
    TState state,
    Func<TState, CancellationToken, Task<T>> factory,
    CancellationToken ct,
    DistributedCacheEntryOptions? options = null);

  /// <summary>
  /// Asynchronously sets or overwrites the value associated with the key.
  /// </summary>
  /// <typeparam name="T">The type of the data being considered.</typeparam>
  /// <param name="key">The key of the entry to create.</param>
  /// <param name="value">The value to assign for this cache entry.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">Additional options for this cache entry.</param>
  Task SetAsync<T>(
    string key,
    T value,
    CancellationToken ct,
    DistributedCacheEntryOptions? options = null);

  /// <summary>
  /// Asynchronously tries to get the value associated with the key if it exists.
  /// </summary>
  /// <typeparam name="T">The type of the data being considered.</typeparam>
  /// <param name="key">The key of the entry to look for.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A tuple, either "true" and the data from cache if found or "false" and a default value if not found.</returns>
  Task<(bool, T?)> TryGetValueAsync<T>(string key, CancellationToken ct);
}

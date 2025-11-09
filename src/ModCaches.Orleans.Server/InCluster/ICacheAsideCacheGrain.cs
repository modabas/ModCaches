namespace ModCaches.Orleans.Server.InCluster;

/// <summary>
/// Represents base methods of an in-cluster cache grain.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public interface ICacheAsideCacheGrain<TValue> : IBaseCacheGrain<TValue>
  where TValue : notnull
{
  /// <summary>
  /// Asynchronously tries to get the cached value associated if it exists, resetting its sliding expiration timeout (if any).
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A record containing either IsFound: "true" and Value: the data from cache, on cache hit, or IsFound: "false" and Value: a default value, on cache miss.</returns>
  Task<TryGetResult<TValue>> TryGetAsync(CancellationToken ct);

  /// <summary>
  /// Asynchronously tries to get the cached value associated if it exists, without resetting its sliding expiration timeout (if any).
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A record containing either IsFound: "true" and Value: the data from cache, on cache hit, or IsFound: "false" and Value: a default value, on cache miss.</returns>
  Task<TryPeekResult<TValue>> TryPeekAsync(CancellationToken ct);

  /// <summary>
  /// Sets the cached value.
  /// </summary>
  /// <param name="value">The value to store in the cache.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>Value stored in cache.</returns>
  Task<TValue> SetAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null);
}

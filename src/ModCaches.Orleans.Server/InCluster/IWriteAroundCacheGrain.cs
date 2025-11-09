namespace ModCaches.Orleans.Server.InCluster;

public interface IWriteAroundCacheGrain<TValue> : IBaseCacheGrain<TValue>
  where TValue : notnull
{
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

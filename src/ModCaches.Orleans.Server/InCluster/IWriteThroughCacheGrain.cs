namespace ModCaches.Orleans.Server.InCluster;

public interface IWriteThroughCacheGrain<TValue> : ICacheGrain<TValue>
  where TValue : notnull
{
  /// <summary>
  /// Performs write-through operation and sets the cached value.
  /// </summary>
  /// <param name="value">The value to store in the cache.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>Value stored in cache.</returns>
  Task<TValue> SetAndWriteAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null);
}

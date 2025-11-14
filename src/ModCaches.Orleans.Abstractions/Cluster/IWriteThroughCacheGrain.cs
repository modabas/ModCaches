namespace ModCaches.Orleans.Abstractions.Cluster;

/// <summary>
/// Represents a cluster cache grain that implements write-through caching strategy.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
/// <typeparam name="TStoreArgs">Type of additional arguments to be into methods operating on underlying data store.</typeparam>
public interface IWriteThroughCacheGrain<TValue, TStoreArgs> : IWriteThroughCacheGrain<TValue>
  where TValue : notnull
  where TStoreArgs : notnull
{
  /// <summary>
  /// Performs write-through operation and sets the cached value utilizing WriteToStoreAsync method. WriteToStoreAsync method must be overridden and implemented in the derived class.
  /// </summary>
  /// <param name="args">Parameters for underlying write-through operations.</param>
  /// <param name="value">The value to store in the cache.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>Value stored in cache.</returns>
  Task<TValue> SetAndWriteAsync(
    TStoreArgs? args,
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null);

  /// <summary>
  /// Performs write-through removal of the cached value utilizing DeleteFromStoreAsync method. DeleteFromStoreAsync method must be overridden and implemented in the derived class.
  /// </summary>
  /// <param name="args">Parameters for underlying write-through operations.</param>
  /// <param name="ct"></param>
  /// <returns></returns>
  Task RemoveAndDeleteAsync(
    TStoreArgs? args,
    CancellationToken ct);
}


/// <summary>
/// Represents a cluster cache grain that implements write-through caching strategy.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public interface IWriteThroughCacheGrain<TValue> : ICacheGrain<TValue>
  where TValue : notnull
{
  /// <summary>
  /// Performs write-through operation and sets the cached value utilizing WriteToStoreAsync method. WriteToStoreAsync method must be overridden and implemented in the derived class.
  /// </summary>
  /// <param name="value">The value to store in the cache.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>Value stored in cache.</returns>
  Task<TValue> SetAndWriteAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null);

  /// <summary>
  /// Performs write-through removal of the cached value utilizing DeleteFromStoreAsync method. DeleteFromStoreAsync method must be overridden and implemented in the derived class.
  /// </summary>
  /// <param name="ct"></param>
  /// <returns></returns>
  Task RemoveAndDeleteAsync(
    CancellationToken ct);
}

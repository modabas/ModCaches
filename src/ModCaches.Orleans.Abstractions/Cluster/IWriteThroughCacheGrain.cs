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
  /// Performs update of the backing data store and sets the cached value. Utilizes WriteToStoreAsync method to update backing data store. WriteToStoreAsync method must be overridden and implemented in the derived class.
  /// </summary>
  /// <param name="args">Parameters for underlying operations from backing data store.</param>
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
  /// Performs deletion from backing data store and removal from cache. Utilizes DeleteFromStoreAsync method for deleting from backing data store. DeleteFromStoreAsync method must be overridden and implemented in the derived class.
  /// </summary>
  /// <param name="args">Parameters for underlying operations from backing data store.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
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
  /// Performs update of the backing data store and sets the cached value. Utilizes WriteToStoreAsync method to update backing data store. WriteToStoreAsync method must be overridden and implemented in the derived class.
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
  /// Performs deletion from backing data store and removal from cache. Utilizes DeleteFromStoreAsync method for deleting from backing data store. DeleteFromStoreAsync method must be overridden and implemented in the derived class.
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns></returns>
  Task RemoveAndDeleteAsync(
    CancellationToken ct);
}

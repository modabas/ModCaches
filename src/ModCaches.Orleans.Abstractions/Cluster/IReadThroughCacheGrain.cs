namespace ModCaches.Orleans.Abstractions.Cluster;

/// <summary>
/// Represents a cluster cache grain that implements read-through caching strategy.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
/// <typeparam name="TStoreArgs">Type of argument to be used during cache value generation.</typeparam>
public interface IReadThroughCacheGrain<TValue, TStoreArgs> : IReadThroughCacheGrain<TValue>
  where TValue : notnull
  where TStoreArgs : notnull
{
  /// <summary>
  /// Either gets the cached value if it exists, or creates a new entry from backing data store otherwise. Utilizes CreateFromStoreAsync method to read from backing data store. CreateFromStoreAsync method must be overridden and implemented in the derived class.
  /// </summary>
  /// <param name="args">Parameters for underlying operations from backing data store.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>The data, either from cache or the underlying value generation method.</returns>
  Task<TValue> GetOrCreateAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null);

  /// <summary>
  /// Creates a new entry from backing data store. Utilizes CreateFromStoreAsync method to read from backing data store. CreateFromStoreAsync method must be overridden and implemented in the derived class.
  /// </summary>
  /// <param name="args">Parameters for underlying operations from backing data store.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>The data from the underlying value generation method.</returns>
  Task<TValue> CreateAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null);
}

/// <summary>
/// Represents a cluster cache grain that implements read-through caching strategy.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public interface IReadThroughCacheGrain<TValue> : ICacheGrain<TValue>
  where TValue : notnull
{
  /// <summary>
  /// Either gets the cached value if it exists, or creates a new entry from backing data store otherwise. Utilizes CreateFromStoreAsync method to read from backing data store. CreateFromStoreAsync method must be overridden and implemented in the derived class.
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>The data, either from cache or the underlying value generation method.</returns>
  Task<TValue> GetOrCreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null);

  /// <summary>
  /// Creates a new entry from backing data store. Utilizes CreateFromStoreAsync method to read from backing data store. CreateFromStoreAsync method must be overridden and implemented in the derived class.
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>The data from the underlying value generation method.</returns>
  Task<TValue> CreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null);
}

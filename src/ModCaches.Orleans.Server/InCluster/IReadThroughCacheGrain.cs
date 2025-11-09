namespace ModCaches.Orleans.Server.InCluster;

/// <summary>
/// Represents an in-cluster cache grain that implements read-through caching strategy.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
/// <typeparam name="TCreateArgs">Type of argument to be used during cache value generation.</typeparam>
public interface IReadThroughCacheGrain<TValue, TCreateArgs> : IBaseCacheGrain<TValue>
  where TValue : notnull
  where TCreateArgs : notnull
{
  /// <summary>
  /// Asynchronously gets the cached value if it exists, or generates a new entry using implemented value generation method otherwise.
  /// </summary>
  /// <param name="createArgs">Arguments to be passed to value generation method.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>The data, either from cache or the underlying value generation method.</returns>
  Task<TValue> GetOrCreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null);

  /// <summary>
  /// Asynchronously generates a new entry using implemented value generation method.
  /// </summary>
  /// <param name="createArgs">Arguments to be passed to value generation method.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>The data from the underlying value generation method.</returns>
  Task<TValue> CreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null);
}

/// <summary>
/// Represents an in-cluster cache grain that implements read-through caching strategy.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public interface IReadThroughCacheGrain<TValue> : IBaseCacheGrain<TValue>
  where TValue : notnull
{
  /// <summary>
  /// Asynchronously gets the cached value if it exists, or generates a new entry using implemented value generation method otherwise.
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>The data, either from cache or the underlying value generation method.</returns>
  Task<TValue> GetOrCreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null);

  /// <summary>
  /// Asynchronously generates a new entry using implemented value generation method.
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>The data from the underlying value generation method.</returns>
  Task<TValue> CreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null);
}

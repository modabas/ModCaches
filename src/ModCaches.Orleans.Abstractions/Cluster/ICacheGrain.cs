using ModResults;

namespace ModCaches.Orleans.Abstractions.Cluster;

/// <summary>
/// Represents base methods of a cluster cache grain. For Cache-Aside and Write-Around caching strategies.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public interface ICacheGrain<TValue> : IGrainWithStringKey
  where TValue : notnull
{
  /// <summary>
  /// Refreshes the value in the cache, resetting its sliding expiration timeout (if any).
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A <see cref="Result"/> that represents the outcome.</returns>
  Task<Result> RefreshAsync(CancellationToken ct);

  /// <summary>
  /// Removes the cached value.
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
  Task RemoveAsync(CancellationToken ct);

  /// <summary>
  /// Asynchronously tries to get the cached value associated if it exists, resetting its sliding expiration timeout (if any).
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A <see cref="Result{TValue}"/> that represents the outcome.</returns>
  Task<Result<TValue>> TryGetAsync(CancellationToken ct);

  /// <summary>
  /// Asynchronously tries to get the cached value associated if it exists, without resetting its sliding expiration timeout (if any).
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A <see cref="Result{TValue}"/> that represents the outcome.</returns>
  Task<Result<TValue>> TryPeekAsync(CancellationToken ct);

  /// <summary>
  /// Sets the cached value.
  /// </summary>
  /// <param name="value">The value to store in the cache.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
  Task SetAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null);
}

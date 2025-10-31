namespace ModCaches.Orleans.Server.InCluster;

/// <summary>
/// Represents an in-cluster cache grain.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public interface IBaseInClusterCacheGrain<TValue> : IGrainWithStringKey
  where TValue : notnull
{
  /// <summary>
  /// Refreshes the value in the cache, resetting its sliding expiration timeout (if any).
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
  Task<bool> RefreshAsync(CancellationToken ct);

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
  /// <returns>A tuple, either "true" and the data from cache if found or "false" and a default value if not found.</returns>
  Task<(bool, TValue?)> TryGetAsync(CancellationToken ct);

  /// <summary>
  /// Asynchronously tries to get the cached value associated if it exists, without resetting its sliding expiration timeout (if any).
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A tuple, either "true" and the data from cache if found or "false" and a default value if not found.</returns>
  Task<(bool, TValue?)> TryPeekAsync(CancellationToken ct);

  /// <summary>
  /// Sets the cached value.
  /// </summary>
  /// <param name="value">The value to set in the cache.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
  Task SetAsync(
    TValue value,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null);
}

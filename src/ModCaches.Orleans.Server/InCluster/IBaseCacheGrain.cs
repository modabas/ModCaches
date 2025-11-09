namespace ModCaches.Orleans.Server.InCluster;

/// <summary>
/// Represents base and common methods of an in-cluster cache grain.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public interface IBaseCacheGrain<TValue> : IGrainWithStringKey
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
}

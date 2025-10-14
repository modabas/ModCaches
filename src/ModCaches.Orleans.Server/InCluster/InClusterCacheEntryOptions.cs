namespace ModCaches.Orleans.Server.InCluster;

/// <summary>
/// Provides the cache options for the cache entry in an in-cluster cache grain.
/// </summary>
[GenerateSerializer]
public class InClusterCacheEntryOptions
{
  /// <summary>
  /// Gets or sets an absolute expiration date for the cache entry.
  /// </summary>
  [Id(0)]
  public DateTimeOffset? AbsoluteExpiration { get; set; }
  /// <summary>
  /// Gets or sets an absolute expiration time, relative to now.
  /// </summary>
  [Id(1)]
  public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
  /// <summary>
  /// Gets or sets how long a cache entry can be inactive (for example, not accessed) before it will be removed.
  /// This will not extend the entry lifetime beyond the absolute expiration (if set).
  /// </summary>
  [Id(2)]
  public TimeSpan? SlidingExpiration { get; set; }
}

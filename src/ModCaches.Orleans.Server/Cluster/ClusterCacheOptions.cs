namespace ModCaches.Orleans.Server.Cluster;

/// <summary>
/// Provides the default cache options for cluster cache grains.
/// </summary>
public class ClusterCacheOptions
{
  /// <summary>
  /// Gets or sets an absolute expiration date for the cache entry.
  /// </summary>
  public DateTimeOffset? AbsoluteExpiration { get; set; }
  /// <summary>
  /// Gets or sets an absolute expiration time for the cache entry, relative to now.
  /// </summary>
  public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
  /// <summary>
  /// Gets or sets how long a cache entry can be inactive (for example, not accessed) before it will be removed.
  /// This will not extend the entry lifetime beyond the absolute expiration (if set).
  /// </summary>
  public TimeSpan? SlidingExpiration { get; set; }
}

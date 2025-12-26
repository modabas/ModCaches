namespace ModCaches.ExtendedDistributedCache;

/// <summary>
/// Provides the options for <see cref="DefaultExtendedDistributedCache"/>.
/// </summary>
public class ExtendedDistributedCacheOptions
{
  /// <summary>
  /// Gets or sets an absolute expiration date for the cache entry.
  /// </summary>
  public DateTimeOffset? AbsoluteExpiration { get; set; }

  /// <summary>
  /// Gets or sets an absolute expiration time, relative to now.
  /// </summary>
  public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

  /// <summary>
  /// Gets or sets how long a cache entry can be inactive (for example, not accessed) before it will be removed.
  /// This will not extend the entry lifetime beyond the absolute expiration (if set).
  /// </summary>
  public TimeSpan? SlidingExpiration { get; set; }

  /// <summary>
  /// Capacity of the LRU (least-recently-used) locks cache used for cache stampede protection.
  /// </summary>
  public int MaxLocks { get; set; } = DefaultMaxLocks; // Default maximum items in internal lock cache

  /// <summary>
  /// Default capacity of the LRU (least-recently-used) locks cache used for cache stampede protection.
  /// </summary>
  public const int DefaultMaxLocks = 8192;
}

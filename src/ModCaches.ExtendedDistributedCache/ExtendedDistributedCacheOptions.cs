namespace ModCaches.ExtendedDistributedCache;

public class ExtendedDistributedCacheOptions
{
  public DateTimeOffset? AbsoluteExpiration { get; set; }
  public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; } = TimeSpan.FromMinutes(10); // Default to 10 minutes
  public TimeSpan? SlidingExpiration { get; set; }
  public int MaxLocks { get; set; } = DefaultMaxLocks; // Default maximum items in internal lock cache

  public const int DefaultMaxLocks = 8192;
}

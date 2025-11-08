namespace ModCaches.Orleans.Server.InCluster;
/// <summary>
/// Provides the cache options for the cache entry in an in-cluster cache grain.
/// </summary>
/// <param name="AbsoluteExpiration">
/// Gets or sets an absolute expiration date for the cache entry.
/// </param>
/// <param name="AbsoluteExpirationRelativeToNow">
/// Gets or sets an absolute expiration time, relative to now.
/// </param>
/// <param name="SlidingExpiration">
/// Gets or sets how long a cache entry can be inactive (for example, not accessed) before it will be removed.
/// This will not extend the entry lifetime beyond the absolute expiration (if set).
/// </param>

[GenerateSerializer]
public record CacheGrainEntryOptions(
  DateTimeOffset? AbsoluteExpiration,
  TimeSpan? AbsoluteExpirationRelativeToNow,
  TimeSpan? SlidingExpiration);

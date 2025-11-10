using ModCaches.Orleans.Abstractions.Cluster;

namespace ModCaches.Orleans.Server.Cluster;
internal static class ClusterCacheOptionsExtensions
{
  public static CacheGrainEntryOptions ToCacheGrainEntryOptions(this ClusterCacheOptions options)
  {
    return new CacheGrainEntryOptions(
        AbsoluteExpiration: options.AbsoluteExpiration,
        AbsoluteExpirationRelativeToNow: options.AbsoluteExpirationRelativeToNow,
        SlidingExpiration: options.SlidingExpiration);
  }
}

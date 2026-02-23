using ModCaches.Orleans.Abstractions.Cluster;

namespace ModCaches.Orleans.Server.Cluster;

internal static class ClusterCacheOptionsExtensions
{
  extension(ClusterCacheOptions options)
  {
    public CacheGrainEntryOptions ToCacheGrainEntryOptions()
    {
      return new CacheGrainEntryOptions(
          AbsoluteExpiration: options.AbsoluteExpiration,
          AbsoluteExpirationRelativeToNow: options.AbsoluteExpirationRelativeToNow,
          SlidingExpiration: options.SlidingExpiration);
    }
  }
}

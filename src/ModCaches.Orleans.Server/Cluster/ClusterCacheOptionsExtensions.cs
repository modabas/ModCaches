using ModCaches.Orleans.Abstractions.Cluster;

namespace ModCaches.Orleans.Server.Cluster;

internal static class ClusterCacheOptionsExtensions
{
  extension(ClusterCacheOptions options)
  {
    public ClusterCacheEntryOptions ToClusterCacheEntryOptions()
    {
      return new ClusterCacheEntryOptions(
          AbsoluteExpiration: options.AbsoluteExpiration,
          AbsoluteExpirationRelativeToNow: options.AbsoluteExpirationRelativeToNow,
          SlidingExpiration: options.SlidingExpiration);
    }
  }
}

using ModCaches.OrleansCaches.Common;

namespace ModCaches.OrleansCaches.InCluster;
internal static class InClusterCacheEntryOptionsExtensions
{
  public static CacheEntryOptions ToOrleansCacheEntryOptions(this InClusterCacheEntryOptions options)
  {
    return new CacheEntryOptions
    {
      AbsoluteExpiration = options.AbsoluteExpiration,
      AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
      SlidingExpiration = options.SlidingExpiration
    };
  }
}

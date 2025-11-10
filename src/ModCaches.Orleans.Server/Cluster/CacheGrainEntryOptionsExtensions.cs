using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Abstractions.Common;

namespace ModCaches.Orleans.Server.Cluster;
internal static class CacheGrainEntryOptionsExtensions
{
  public static CacheEntryOptions ToOrleansCacheEntryOptions(this CacheGrainEntryOptions options)
  {
    return new CacheEntryOptions(options.AbsoluteExpiration, options.AbsoluteExpirationRelativeToNow, options.SlidingExpiration);
  }
}

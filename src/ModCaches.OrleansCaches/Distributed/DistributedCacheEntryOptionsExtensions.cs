using Microsoft.Extensions.Caching.Distributed;
using ModCaches.OrleansCaches.Common;

namespace ModCaches.OrleansCaches.Distributed;

internal static class DistributedCacheEntryOptionsExtensions
{
  public static CacheEntryOptions ToOrleansCacheEntryOptions(this DistributedCacheEntryOptions options)
  {
    return new CacheEntryOptions
    {
      AbsoluteExpiration = options.AbsoluteExpiration,
      AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
      SlidingExpiration = options.SlidingExpiration
    };
  }
}

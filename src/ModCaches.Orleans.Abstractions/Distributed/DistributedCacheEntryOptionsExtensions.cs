using Microsoft.Extensions.Caching.Distributed;
using ModCaches.Orleans.Abstractions.Common;

namespace ModCaches.Orleans.Abstractions.Distributed;

internal static class DistributedCacheEntryOptionsExtensions
{
  public static CacheEntryOptions ToOrleansCacheEntryOptions(this DistributedCacheEntryOptions options)
  {
    return new CacheEntryOptions(
      AbsoluteExpiration: options.AbsoluteExpiration,
      AbsoluteExpirationRelativeToNow: options.AbsoluteExpirationRelativeToNow,
      SlidingExpiration: options.SlidingExpiration);
  }
}

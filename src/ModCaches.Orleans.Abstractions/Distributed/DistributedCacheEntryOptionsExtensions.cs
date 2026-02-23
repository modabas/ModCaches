using Microsoft.Extensions.Caching.Distributed;
using ModCaches.Orleans.Abstractions.Common;

namespace ModCaches.Orleans.Abstractions.Distributed;

internal static class DistributedCacheEntryOptionsExtensions
{
  extension(DistributedCacheEntryOptions options)
  {
    public CacheEntryOptions ToOrleansCacheEntryOptions()
    {
      return new CacheEntryOptions(
        AbsoluteExpiration: options.AbsoluteExpiration,
        AbsoluteExpirationRelativeToNow: options.AbsoluteExpirationRelativeToNow,
        SlidingExpiration: options.SlidingExpiration);
    }
  }
}

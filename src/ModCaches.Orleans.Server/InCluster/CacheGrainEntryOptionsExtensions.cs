using ModCaches.Orleans.Abstractions.Common;

namespace ModCaches.Orleans.Server.InCluster;
internal static class CacheGrainEntryOptionsExtensions
{
  public static CacheEntryOptions ToOrleansCacheEntryOptions(this CacheGrainEntryOptions options)
  {
    return new CacheEntryOptions
    {
      AbsoluteExpiration = options.AbsoluteExpiration,
      AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
      SlidingExpiration = options.SlidingExpiration
    };
  }
}

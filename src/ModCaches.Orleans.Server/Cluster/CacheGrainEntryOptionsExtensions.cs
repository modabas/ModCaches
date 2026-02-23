using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Abstractions.Common;

namespace ModCaches.Orleans.Server.Cluster;

internal static class CacheGrainEntryOptionsExtensions
{
  extension(CacheGrainEntryOptions options)
  {
    public CacheEntryOptions ToOrleansCacheEntryOptions()
    {
      return new CacheEntryOptions(
        options.AbsoluteExpiration,
        options.AbsoluteExpirationRelativeToNow,
        options.SlidingExpiration);
    }
  }
}

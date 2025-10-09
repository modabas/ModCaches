using System.Collections.Immutable;
using ModCaches.OrleansCaches.Common;

namespace ModCaches.OrleansCaches.Distributed;
internal static class CacheEntryExtensions
{
  public static DistributedCacheState ToState(this CacheEntry<ImmutableArray<byte>> cacheEntry)
  {
    var entryData = cacheEntry.GetStoredData();
    return new()
    {
      Value = entryData.Value,
      AbsoluteExpiration = entryData.AbsoluteExpiration,
      LastAccessed = entryData.LastAccessed,
      SlidingExpiration = entryData.SlidingExpiration
    };
  }
}

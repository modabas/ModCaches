using ModCaches.OrleansCaches.Common;

namespace ModCaches.OrleansCaches.InCluster;
internal static class CacheEntryExtensions
{
  public static InClusterCacheState<TValue> ToState<TValue>(this CacheEntry<TValue> cacheEntry)
    where TValue : notnull
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

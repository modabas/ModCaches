using ModCaches.Orleans.Server.Common;

namespace ModCaches.Orleans.Server.Cluster;

internal static class CacheEntryExtensions
{
  extension<TValue>(CacheEntry<TValue> cacheEntry) where TValue : notnull
  {
    public CacheState<TValue> ToState()
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
}

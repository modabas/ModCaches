using ModResults;

namespace ModCaches.Orleans.Server.Cluster;

public static class ClusterCacheEntryExtensions
{
  extension<TValue>(ClusterCacheEntry<TValue> entry) where TValue : notnull
  {
    /// <summary>
    /// Wraps the specified cache grain entry in a successful result object.
    /// </summary>
    /// <returns>A successful result containing the specified cache grain entry.</returns>
    public Result<ClusterCacheEntry<TValue>> ToResult()
    {
      return Result.Ok(entry);
    }
  }
}

using ModResults;

namespace ModCaches.Orleans.Server.Cluster;

public static class CacheGrainEntryExtensions
{
  extension<TValue>(CacheGrainEntry<TValue> entry) where TValue : notnull
  {
    /// <summary>
    /// Wraps the specified cache grain entry in a successful result object.
    /// </summary>
    /// <returns>A successful result containing the specified cache grain entry.</returns>
    public Result<CacheGrainEntry<TValue>> ToResult()
    {
      return Result.Ok(entry);
    }
  }
}

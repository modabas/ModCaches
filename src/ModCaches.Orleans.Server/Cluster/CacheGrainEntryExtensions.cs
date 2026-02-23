using ModResults;

namespace ModCaches.Orleans.Server.Cluster;

public static class CacheGrainEntryExtensions
{
  /// <summary>
  /// Wraps the specified cache grain entry in a successful result object.
  /// </summary>
  /// <typeparam name="TValue">The type of the value contained in the cache grain entry. Must not be null.</typeparam>
  /// <param name="entry">The cache grain entry to wrap in a result.</param>
  /// <returns>A successful result containing the specified cache grain entry.</returns>
  public static Result<CacheGrainEntry<TValue>> ToResult<TValue>(
    this CacheGrainEntry<TValue> entry)
    where TValue : notnull
  {
    return Result.Ok(entry);
  }
}

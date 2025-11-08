namespace ModCaches.Orleans.Server.InCluster;
internal static class InClusterCacheOptionsExtensions
{
  public static CacheGrainEntryOptions ToCacheGrainEntryOptions(this InClusterCacheOptions options)
  {
    return new CacheGrainEntryOptions(
        AbsoluteExpiration: options.AbsoluteExpiration,
        AbsoluteExpirationRelativeToNow: options.AbsoluteExpirationRelativeToNow,
        SlidingExpiration: options.SlidingExpiration);
  }
}

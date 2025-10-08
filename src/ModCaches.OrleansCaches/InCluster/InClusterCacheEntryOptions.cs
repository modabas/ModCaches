using Orleans;

namespace ModCaches.OrleansCaches.InCluster;

[GenerateSerializer]
public class InClusterCacheEntryOptions
{
  [Id(0)]
  public DateTimeOffset? AbsoluteExpiration { get; set; }
  [Id(1)]
  public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
  [Id(2)]
  public TimeSpan? SlidingExpiration { get; set; }
}

namespace ModCaches.Orleans.Server.InCluster;

[GenerateSerializer]
public class InClusterCacheState<T> where T : notnull
{
  [Id(0)]
  public required T Value { get; set; }
  [Id(1)]
  public DateTimeOffset? AbsoluteExpiration { get; set; }
  [Id(2)]
  public TimeSpan? SlidingExpiration { get; set; }
  [Id(3)]
  public DateTimeOffset LastAccessed { get; set; }
}

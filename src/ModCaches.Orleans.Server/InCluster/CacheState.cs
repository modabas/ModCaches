namespace ModCaches.Orleans.Server.InCluster;

/// <summary>
/// Holds the persistent state for an in-cluster cache grain.
/// </summary>
/// <typeparam name="TValue"></typeparam>
[GenerateSerializer]
public class CacheState<TValue> where TValue : notnull
{
  [Id(0)]
  public required TValue Value { get; set; }
  [Id(1)]
  public DateTimeOffset? AbsoluteExpiration { get; set; }
  [Id(2)]
  public TimeSpan? SlidingExpiration { get; set; }
  [Id(3)]
  public DateTimeOffset LastAccessed { get; set; }
}

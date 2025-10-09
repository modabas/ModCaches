using System.Collections.Immutable;
using Orleans;

namespace ModCaches.OrleansCaches.Distributed;

[GenerateSerializer]
public class DistributedCacheState
{
  [Id(0)]
  public required ImmutableArray<byte> Value { get; set; }
  [Id(1)]
  public DateTimeOffset? AbsoluteExpiration { get; set; }
  [Id(2)]
  public TimeSpan? SlidingExpiration { get; set; }
  [Id(3)]
  public DateTimeOffset LastAccessed { get; set; }
}

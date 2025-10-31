using ModCaches.Orleans.Abstractions.Distributed;

namespace ModCaches.Orleans.Server.Distributed;

internal class VolatileDistributedCacheGrain : BaseDistributedCacheGrain, IVolatileDistributedCacheGrain
{
  public VolatileDistributedCacheGrain(TimeProvider timeProvider)
    : base(timeProvider)
  {
  }
}

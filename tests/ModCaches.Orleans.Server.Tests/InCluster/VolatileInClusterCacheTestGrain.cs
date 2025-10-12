using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

internal interface IVolatileInClusterCacheTestGrain : IInClusterCacheGrain<string>;
internal class VolatileInClusterCacheTestGrain : VolatileInClusterCacheGrain<string>, IVolatileInClusterCacheTestGrain
{
  public VolatileInClusterCacheTestGrain(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override Task<string> GenerateValueAsync(InClusterCacheEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult("volatile in cluster cache");
  }
}

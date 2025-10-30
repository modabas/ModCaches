using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

internal interface IVolatileInClusterCacheTestGrainWithCreateArgs : IInClusterCacheGrain<string, int>;
internal class VolatileInClusterCacheTestGrainWithCreateArgs : VolatileInClusterCacheGrain<string, int>, IVolatileInClusterCacheTestGrainWithCreateArgs
{
  public VolatileInClusterCacheTestGrainWithCreateArgs(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override Task<string> GenerateValueAsync(int args, InClusterCacheEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult($"volatile in cluster cache {args}");
  }
}

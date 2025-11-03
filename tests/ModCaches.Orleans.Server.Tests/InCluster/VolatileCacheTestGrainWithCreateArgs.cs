using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

internal interface IVolatileCacheTestGrainWithCreateArgs : ICacheGrain<string, int>;
internal class VolatileCacheTestGrainWithCreateArgs : VolatileCacheGrain<string, int>, IVolatileCacheTestGrainWithCreateArgs
{
  public VolatileCacheTestGrainWithCreateArgs(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override Task<string> GenerateValueAsync(int args, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult($"volatile in cluster cache {args}");
  }
}

using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

internal interface IVolatileCacheTestGrain : ICacheGrain<string>;
internal class VolatileCacheTestGrain : VolatileCacheGrain<string>, IVolatileCacheTestGrain
{
  public VolatileCacheTestGrain(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override Task<string> GenerateValueAsync(CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult("volatile in cluster cache");
  }
}

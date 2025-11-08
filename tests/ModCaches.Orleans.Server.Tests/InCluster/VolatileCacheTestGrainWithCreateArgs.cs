using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

internal interface IVolatileCacheTestGrainWithCreateArgs : ICacheGrain<string, int>;
internal class VolatileCacheTestGrainWithCreateArgs : VolatileCacheGrain<string, int>, IVolatileCacheTestGrainWithCreateArgs
{
  public VolatileCacheTestGrainWithCreateArgs(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override Task<GenerateEntryResult<string>> GenerateEntryAsync(int args, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new GenerateEntryResult<string>(Value: $"volatile in cluster cache {args}", Options: options));
  }
}

using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Cluster;

namespace ModCaches.Orleans.Server.Tests.Cluster;

internal interface IVolatileCacheTestGrainWithStoreArgs :
  IReadThroughCacheGrain<string, int>,
  ICacheGrain<string>,
  IWriteThroughCacheGrain<string>;

internal class VolatileCacheTestGrainWithStoreArgs : VolatileCacheGrain<string, int>, IVolatileCacheTestGrainWithStoreArgs
{
  public VolatileCacheTestGrainWithStoreArgs(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override Task<CreateResult<string>> CreateFromStoreAsync(int args, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new CreateResult<string>(Value: $"volatile in cluster cache {args}", Options: options));
  }

  protected override Task<WriteResult<string>> WriteToStoreAsync(int args, string value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new WriteResult<string>($"write-through {value}", options));
  }
}

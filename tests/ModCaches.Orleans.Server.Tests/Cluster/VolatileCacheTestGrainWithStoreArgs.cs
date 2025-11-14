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

  protected override Task<ReadThroughResult<string>> ReadThroughAsync(int args, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new ReadThroughResult<string>(Value: $"volatile in cluster cache {args}", Options: options));
  }

  protected override Task<WriteThroughResult<string>> WriteThroughAsync(string value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new WriteThroughResult<string>($"write-through {value}", options));
  }

}

using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Cluster;

namespace ModCaches.Orleans.Server.Tests.Cluster;

internal interface IVolatileCacheTestGrain :
  IReadThroughCacheGrain<string>,
  ICacheGrain<string>,
  IWriteThroughCacheGrain<string>;

internal class VolatileCacheTestGrain : VolatileCacheGrain<string>, IVolatileCacheTestGrain
{
  public VolatileCacheTestGrain(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override Task<ReadThroughResult<string>> ReadThroughAsync(CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new ReadThroughResult<string>("volatile in cluster cache", options));
  }

  protected override Task<WriteThroughResult<string>> WriteThroughAsync(string value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new WriteThroughResult<string>($"write-through {value}", options));
  }

}

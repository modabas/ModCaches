using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Cluster;
using ModResults;

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

  protected override Task<Result<CreateRecord<string>>> CreateFromStoreAsync(CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(Result.Ok(new CreateRecord<string>("volatile in cluster cache", options)));
  }

  protected override Task<Result<WriteRecord<string>>> WriteToStoreAsync(string value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(Result.Ok(new WriteRecord<string>($"write-through {value}", options)));
  }
}

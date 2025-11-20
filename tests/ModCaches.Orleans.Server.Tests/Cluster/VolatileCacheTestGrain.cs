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

  protected override Task<Result<CreatedItem<string>>> CreateFromStoreAsync(CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(Result.Ok(new CreatedItem<string>("volatile in cluster cache", options)));
  }

  protected override Task<Result<WrittenItem<string>>> WriteToStoreAsync(string value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(Result.Ok(new WrittenItem<string>($"write-through {value}", options)));
  }
}

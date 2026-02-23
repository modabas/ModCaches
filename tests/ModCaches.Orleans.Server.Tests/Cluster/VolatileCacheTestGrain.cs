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

  protected override async Task<Result<CacheGrainEntry<string>>> CreateFromStoreAsync(
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    return CacheGrainEntry.Create("volatile in cluster cache", options);
  }

  protected override async Task<Result<CacheGrainEntry<string>>> WriteToStoreAsync(
    string value,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    return CacheGrainEntry.Create($"write-through {value}", options);
  }
}

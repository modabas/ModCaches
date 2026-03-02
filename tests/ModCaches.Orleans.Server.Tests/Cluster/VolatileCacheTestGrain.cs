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

  protected override async Task<Result<ClusterCacheEntry<string>>> CreateFromStoreAsync(
    ClusterCacheEntryOptions options,
    CancellationToken ct)
  {
    return ClusterCacheEntry.Create("volatile in cluster cache", options);
  }

  protected override async Task<Result<ClusterCacheEntry<string>>> WriteToStoreAsync(
    string value,
    ClusterCacheEntryOptions options,
    CancellationToken ct)
  {
    return ClusterCacheEntry.Create($"write-through {value}", options);
  }
}

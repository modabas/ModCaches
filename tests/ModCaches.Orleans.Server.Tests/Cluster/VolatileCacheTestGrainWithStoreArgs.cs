using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Cluster;
using ModResults;

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

  protected override async Task<Result<ClusterCacheEntry<string>>> CreateFromStoreAsync(
    int args,
    ClusterCacheEntryOptions options,
    CancellationToken ct)
  {
    return ClusterCacheEntry.Create($"volatile in cluster cache {args}", options);
  }

  protected override async Task<Result<ClusterCacheEntry<string>>> WriteToStoreAsync(
    int args,
    string value,
    ClusterCacheEntryOptions options,
    CancellationToken ct)
  {
    return ClusterCacheEntry.Create($"write-through {value}", options);
  }
}

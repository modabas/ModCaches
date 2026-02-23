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

  protected override async Task<Result<CacheGrainEntry<string>>> CreateFromStoreAsync(
    int args,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    return CacheGrainEntry.Create($"volatile in cluster cache {args}", options);
  }

  protected override async Task<Result<CacheGrainEntry<string>>> WriteToStoreAsync(
    int args,
    string value,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    return CacheGrainEntry.Create($"write-through {value}", options);
  }
}

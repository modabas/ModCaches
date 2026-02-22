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

  protected override async Task<Result<CreateRecord<string>>> CreateFromStoreAsync(
    int args,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    return CreateRecord.From($"volatile in cluster cache {args}", options);
  }

  protected override async Task<Result<WriteRecord<string>>> WriteToStoreAsync(
    int args,
    string value,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    return WriteRecord.From($"write-through {value}", options);
  }
}

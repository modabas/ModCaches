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

  protected override Task<Result<CreatedItem<string>>> CreateFromStoreAsync(int args, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(Result.Ok(new CreatedItem<string>(Value: $"volatile in cluster cache {args}", Options: options)));
  }

  protected override Task<Result<WrittenItem<string>>> WriteToStoreAsync(int args, string value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(Result.Ok(new WrittenItem<string>($"write-through {value}", options)));
  }
}

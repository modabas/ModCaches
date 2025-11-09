using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

internal interface IVolatileCacheTestGrainWithCreateArgs :
  IReadThroughCacheGrain<string, int>,
  ICacheGrain<string>,
  IWriteThroughCacheGrain<string>;

internal class VolatileCacheTestGrainWithCreateArgs : VolatileCacheGrain<string, int>, IVolatileCacheTestGrainWithCreateArgs
{
  public VolatileCacheTestGrainWithCreateArgs(IServiceProvider serviceProvider) : base(serviceProvider)
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

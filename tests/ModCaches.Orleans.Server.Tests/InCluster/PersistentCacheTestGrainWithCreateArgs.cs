using ModCaches.Orleans.Server.InCluster;
using NSubstitute;

namespace ModCaches.Orleans.Server.Tests.InCluster;

internal interface IPersistentCacheTestGrainWithCreateArgs :
  IReadThroughCacheGrain<CacheTestValue ,int>,
  ICacheGrain<CacheTestValue>,
  IWriteThroughCacheGrain<CacheTestValue>;
internal class PersistentCacheTestGrainWithCreateArgs : PersistentCacheGrain<CacheTestValue, int>, IPersistentCacheTestGrainWithCreateArgs
{
  public PersistentCacheTestGrainWithCreateArgs(
    IServiceProvider serviceProvider,
    [PersistentState(nameof(PersistentCacheTestGrainWithCreateArgs))] IPersistentState<CacheState<CacheTestValue>> persistentState) : base(serviceProvider, persistentState)
  {
  }

  protected override Task<ReadThroughResult<CacheTestValue>> ReadThroughAsync(int args, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new ReadThroughResult<CacheTestValue>(new CacheTestValue() { Data = $"persistent in cluster cache {args}" }, options));
  }

  protected override Task<WriteThroughResult<CacheTestValue>> WriteThroughAsync(CacheTestValue value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new WriteThroughResult<CacheTestValue>(new CacheTestValue() { Data = $"write-through {value.Data}" }, options));
  }
}

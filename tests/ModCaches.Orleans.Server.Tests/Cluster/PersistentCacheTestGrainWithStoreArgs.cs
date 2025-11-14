using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Cluster;

namespace ModCaches.Orleans.Server.Tests.Cluster;

internal interface IPersistentCacheTestGrainWithStoreArgs :
  IReadThroughCacheGrain<CacheTestValue, int>,
  ICacheGrain<CacheTestValue>,
  IWriteThroughCacheGrain<CacheTestValue>;
internal class PersistentCacheTestGrainWithStoreArgs : PersistentCacheGrain<CacheTestValue, int>, IPersistentCacheTestGrainWithStoreArgs
{
  public PersistentCacheTestGrainWithStoreArgs(
    IServiceProvider serviceProvider,
    [PersistentState(nameof(PersistentCacheTestGrainWithStoreArgs))] IPersistentState<CacheState<CacheTestValue>> persistentState) : base(serviceProvider, persistentState)
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

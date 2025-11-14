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

  protected override Task<CreateResult<CacheTestValue>> CreateFromStoreAsync(int args, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new CreateResult<CacheTestValue>(new CacheTestValue() { Data = $"persistent in cluster cache {args}" }, options));
  }

  protected override Task<WriteResult<CacheTestValue>> WriteToStoreAsync(int args, CacheTestValue value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new WriteResult<CacheTestValue>(new CacheTestValue() { Data = $"write-through {value.Data}" }, options));
  }
}

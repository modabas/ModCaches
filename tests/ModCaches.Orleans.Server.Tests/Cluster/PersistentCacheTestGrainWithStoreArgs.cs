using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Cluster;
using ModResults;

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

  protected override Task<Result<CreateRecord<CacheTestValue>>> CreateFromStoreAsync(int args, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(Result.Ok(new CreateRecord<CacheTestValue>(new CacheTestValue() { Data = $"persistent in cluster cache {args}" }, options)));
  }

  protected override Task<Result<WriteRecord<CacheTestValue>>> WriteToStoreAsync(int args, CacheTestValue value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(Result.Ok(new WriteRecord<CacheTestValue>(new CacheTestValue() { Data = $"write-through {value.Data}" }, options)));
  }
}

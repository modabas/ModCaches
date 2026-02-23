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

  protected override async Task<Result<CacheGrainEntry<CacheTestValue>>> CreateFromStoreAsync(
    int args,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    return CacheGrainEntry.Create(
      new CacheTestValue() { Data = $"persistent in cluster cache {args}" },
      options);
  }

  protected override async Task<Result<CacheGrainEntry<CacheTestValue>>> WriteToStoreAsync(
    int args,
    CacheTestValue value,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    return CacheGrainEntry.Create(
      new CacheTestValue() { Data = $"write-through {value.Data}" },
      options);
  }
}

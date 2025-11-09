using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

internal interface IPersistentCacheTestGrain :
  IReadThroughCacheGrain<CacheTestValue>,
  ICacheGrain<CacheTestValue>,
  IWriteThroughCacheGrain<CacheTestValue>;
internal class PersistentCacheTestGrain : PersistentCacheGrain<CacheTestValue>, IPersistentCacheTestGrain
{
  public PersistentCacheTestGrain(
    IServiceProvider serviceProvider,
    [PersistentState(nameof(PersistentCacheTestGrain))] IPersistentState<CacheState<CacheTestValue>> persistentState) : base(serviceProvider, persistentState)
  {
  }

  protected override Task<ReadThroughResult<CacheTestValue>> ReadThroughAsync(CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new ReadThroughResult<CacheTestValue>(new CacheTestValue() { Data = "persistent in cluster cache" }, options));
  }

  protected override Task<WriteThroughResult<CacheTestValue>> WriteThroughAsync(CacheTestValue value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new WriteThroughResult<CacheTestValue>(new CacheTestValue() { Data = "write-through persistent in cluster cache" }, options));
  }
}

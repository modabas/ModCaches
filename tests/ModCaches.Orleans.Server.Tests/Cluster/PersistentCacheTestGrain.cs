using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Cluster;

namespace ModCaches.Orleans.Server.Tests.Cluster;

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

  protected override Task<CreateResult<CacheTestValue>> CreateFromStoreAsync(CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new CreateResult<CacheTestValue>(new CacheTestValue() { Data = "persistent in cluster cache" }, options));
  }

  protected override Task<WriteResult<CacheTestValue>> WriteToStoreAsync(CacheTestValue value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new WriteResult<CacheTestValue>(new CacheTestValue() { Data = "write-through persistent in cluster cache" }, options));
  }
}

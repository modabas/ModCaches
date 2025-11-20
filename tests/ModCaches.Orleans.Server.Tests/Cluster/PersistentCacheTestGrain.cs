using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Cluster;
using ModResults;

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

  protected override Task<Result<CreatedItem<CacheTestValue>>> CreateFromStoreAsync(CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(Result.Ok(new CreatedItem<CacheTestValue>(new CacheTestValue() { Data = "persistent in cluster cache" }, options)));
  }

  protected override Task<Result<WrittenItem<CacheTestValue>>> WriteToStoreAsync(CacheTestValue value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(Result.Ok(new WrittenItem<CacheTestValue>(new CacheTestValue() { Data = "write-through persistent in cluster cache" }, options)));
  }
}

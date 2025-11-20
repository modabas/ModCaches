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

  protected override Task<Result<CreateRecord<CacheTestValue>>> CreateFromStoreAsync(CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(Result.Ok(new CreateRecord<CacheTestValue>(new CacheTestValue() { Data = "persistent in cluster cache" }, options)));
  }

  protected override Task<Result<WriteRecord<CacheTestValue>>> WriteToStoreAsync(CacheTestValue value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(Result.Ok(new WriteRecord<CacheTestValue>(new CacheTestValue() { Data = "write-through persistent in cluster cache" }, options)));
  }
}

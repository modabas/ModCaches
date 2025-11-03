using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

internal interface IPersistentCacheTestGrain : ICacheGrain<CacheTestValue>;
internal class PersistentCacheTestGrain : PersistentCacheGrain<CacheTestValue>, IPersistentCacheTestGrain
{
  public PersistentCacheTestGrain(
    IServiceProvider serviceProvider,
    [PersistentState(nameof(PersistentCacheTestGrain))] IPersistentState<CacheState<CacheTestValue>> persistentState) : base(serviceProvider, persistentState)
  {
  }

  protected override Task<CacheTestValue> GenerateValueAsync(CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new CacheTestValue() { Data = "persistent in cluster cache" });
  }
}

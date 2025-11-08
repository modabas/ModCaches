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

  protected override Task<GenerateEntryResult<CacheTestValue>> GenerateEntryAsync(CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new GenerateEntryResult<CacheTestValue>(new CacheTestValue() { Data = "persistent in cluster cache" }, options));
  }
}

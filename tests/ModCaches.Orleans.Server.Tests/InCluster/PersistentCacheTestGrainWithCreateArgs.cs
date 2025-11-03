using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

internal interface IPersistentCacheTestGrainWithCreateArgs : ICacheGrain<CacheTestValue, int>;
internal class PersistentCacheTestGrainWithCreateArgs : PersistentCacheGrain<CacheTestValue, int>, IPersistentCacheTestGrainWithCreateArgs
{
  public PersistentCacheTestGrainWithCreateArgs(
    IServiceProvider serviceProvider,
    [PersistentState(nameof(PersistentCacheTestGrainWithCreateArgs))] IPersistentState<CacheState<CacheTestValue>> persistentState) : base(serviceProvider, persistentState)
  {
  }

  protected override Task<CacheTestValue> GenerateValueAsync(int args, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new CacheTestValue() { Data = $"persistent in cluster cache {args}" });
  }
}

using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

internal interface IPersistentInClusterCacheTestGrain : IInClusterCacheGrain<InClusterTestCacheState>;
internal class PersistentInClusterCacheTestGrain : PersistentInClusterCacheGrain<InClusterTestCacheState>, IPersistentInClusterCacheTestGrain
{
  public PersistentInClusterCacheTestGrain(
    IServiceProvider serviceProvider,
    [PersistentState(nameof(PersistentInClusterCacheTestGrain))] IPersistentState<InClusterCacheState<InClusterTestCacheState>> persistentState) : base(serviceProvider, persistentState)
  {
  }

  protected override Task<InClusterTestCacheState> GenerateValueAsync(InClusterCacheEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new InClusterTestCacheState() { Data = "persistent in cluster cache" });
  }
}

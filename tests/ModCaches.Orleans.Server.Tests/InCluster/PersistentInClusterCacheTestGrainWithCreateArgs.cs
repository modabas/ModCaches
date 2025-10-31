using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

internal interface IPersistentInClusterCacheTestGrainWithCreateArgs : IInClusterCacheGrain<InClusterTestCacheState, int>;
internal class PersistentInClusterCacheTestGrainWithCreateArgs : PersistentInClusterCacheGrain<InClusterTestCacheState, int>, IPersistentInClusterCacheTestGrainWithCreateArgs
{
  public PersistentInClusterCacheTestGrainWithCreateArgs(
    IServiceProvider serviceProvider,
    [PersistentState(nameof(PersistentInClusterCacheTestGrainWithCreateArgs))] IPersistentState<InClusterCacheState<InClusterTestCacheState>> persistentState) : base(serviceProvider, persistentState)
  {
  }

  protected override Task<InClusterTestCacheState> GenerateValueAsync(int args, InClusterCacheEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult(new InClusterTestCacheState() { Data = $"persistent in cluster cache {args}" });
  }
}

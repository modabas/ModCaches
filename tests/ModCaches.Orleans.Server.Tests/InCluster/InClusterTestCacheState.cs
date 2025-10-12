namespace ModCaches.Orleans.Server.Tests.InCluster;

[GenerateSerializer]
internal class InClusterTestCacheState
{
  [Id(0)]
  public string Data { get; set; } = string.Empty;
}

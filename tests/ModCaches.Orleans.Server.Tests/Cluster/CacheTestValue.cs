namespace ModCaches.Orleans.Server.Tests.Cluster;

[GenerateSerializer]
internal class CacheTestValue
{
  [Id(0)]
  public string Data { get; set; } = string.Empty;
}

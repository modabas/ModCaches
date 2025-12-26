using Microsoft.Extensions.DependencyInjection;
using Orleans.TestingHost;

namespace ModCaches.Orleans.Server.Tests;

public sealed class ClusterFixture : IDisposable
{
  public TestCluster Cluster { get; } = new TestClusterBuilder()
    .AddSiloBuilderConfigurator<TestSiloConfigurator>()
    .Build();

  public ClusterFixture() => Cluster.Deploy();

  void IDisposable.Dispose() => Cluster.StopAllSilos();
}

public class TestSiloConfigurator : ISiloConfigurator
{
  public void Configure(ISiloBuilder siloBuilder)
  {
    siloBuilder.AddMemoryGrainStorageAsDefault();
    siloBuilder.ConfigureServices(services =>
    {
      services.AddTransient<GrainIdFactory>();
    });
  }
}

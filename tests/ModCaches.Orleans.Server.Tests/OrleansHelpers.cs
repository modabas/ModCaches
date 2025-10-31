using Microsoft.Extensions.DependencyInjection;
using Orleans.Providers;
using Orleans.Storage;

namespace ModCaches.Orleans.Server.Tests;
internal static class OrleansHelpers
{
  public static GrainIdFactory GetGrainIdFactory(ClusterFixture fixture)
  {
    return fixture.Cluster.GetSiloServiceProvider().GetRequiredService<GrainIdFactory>();
  }

  public static IGrainStorage GetDefaultGrainStorage(ClusterFixture fixture)
  {
    return fixture.Cluster.GetSiloServiceProvider().GetRequiredKeyedService<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);
  }
}

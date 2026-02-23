using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ModCaches.Orleans.Server.Cluster;

public static class ServiceCollectionExtensions
{
  extension(IServiceCollection services)
  {
    /// <summary>
    /// Adds cluster cache grains related services.
    /// </summary>
    /// <param name="setupAction">Action to configure default <see cref="ClusterCacheOptions"/>.</param>
    /// <returns></returns>
    public IServiceCollection AddOrleansClusterCache(
      Action<ClusterCacheOptions>? setupAction = null)
    {
      services.TryAddSingleton(TimeProvider.System);
      Action<ClusterCacheOptions> defaultSetupAction = setupAction is null
        ? (options) => options = new ClusterCacheOptions()
        : (options) => setupAction(options);
      services.Configure(defaultSetupAction);
      return services;
    }
  }
}

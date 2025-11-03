using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ModCaches.Orleans.Server.InCluster;
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Adds in-cluster cache grains related services.
  /// </summary>
  /// <param name="services"></param>
  /// <param name="setupAction">Action to configure default <see cref="InClusterCacheOptions"/>.</param>
  /// <returns></returns>
  public static IServiceCollection AddOrleansInClusterCache(
    this IServiceCollection services,
    Action<InClusterCacheOptions>? setupAction = null)
  {
    services.TryAddSingleton(TimeProvider.System);
    Action<InClusterCacheOptions> defaultSetupAction = setupAction is null
      ? (options) => options = new InClusterCacheOptions()
      : (options) => setupAction(options);
    services.Configure(defaultSetupAction);
    return services;
  }
}

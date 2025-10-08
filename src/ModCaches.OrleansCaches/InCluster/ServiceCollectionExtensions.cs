using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ModCaches.OrleansCaches.InCluster;
public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddOrleansInClusterCache(
    this IServiceCollection services,
    Action<InClusterCacheEntryOptions>? setupAction = null)
  {
    services.TryAddSingleton(TimeProvider.System);
    Action<InClusterCacheEntryOptions> defaultSetupAction = setupAction is null
      ? (options) => options = new InClusterCacheEntryOptions()
      : (options) => setupAction(options);
    services.Configure(defaultSetupAction);
    return services;
  }
}

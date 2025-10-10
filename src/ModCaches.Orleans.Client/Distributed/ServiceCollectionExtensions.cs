using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ModCaches.Orleans.Client.Distributed;
public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddRemoteOrleansVolatileDistributedCache(
    this IServiceCollection services,
    object? cacheDiKey = null,
    ServiceLifetime lifetime = ServiceLifetime.Singleton)
  {
    services.TryAdd(new ServiceDescriptor(
      typeof(IDistributedCache),
      cacheDiKey,
      typeof(RemoteOrleansVolatileCache),
      lifetime));

    return services;
  }

  public static IServiceCollection AddRemoteOrleansPersistentDistributedCache(
    this IServiceCollection services,
    object? cacheDiKey = null,
    ServiceLifetime lifetime = ServiceLifetime.Singleton)
  {
    services.TryAdd(new ServiceDescriptor(
      typeof(IDistributedCache),
      cacheDiKey,
      typeof(RemoteOrleansPersistentCache),
      lifetime));

    return services;
  }
}

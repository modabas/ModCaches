using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ModCaches.OrleansCaches.Distributed;
public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddCoHostedOrleansVolatileDistributedCache(
    this IServiceCollection services,
    object? cacheDiKey = null,
    ServiceLifetime lifetime = ServiceLifetime.Singleton)
  {
    services.TryAddSingleton(TimeProvider.System);
    services.TryAdd(new ServiceDescriptor(
      typeof(IDistributedCache),
      cacheDiKey,
      typeof(CoHostedOrleansVolatileCache),
      lifetime));

    return services;
  }

  public static IServiceCollection AddCoHostedOrleansPersistentDistributedCache(
    this IServiceCollection services,
    object? cacheDiKey = null,
    ServiceLifetime lifetime = ServiceLifetime.Singleton)
  {
    services.TryAddSingleton(TimeProvider.System);
    services.TryAdd(new ServiceDescriptor(
      typeof(IDistributedCache),
      cacheDiKey,
      typeof(CoHostedOrleansPersistentCache),
      lifetime));

    return services;
  }

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

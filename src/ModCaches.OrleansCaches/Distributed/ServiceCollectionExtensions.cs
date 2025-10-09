using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ModCaches.OrleansCaches.Distributed;
public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddCoHostedOrleansDistributedCache(
    this IServiceCollection services,
    object? cacheDiKey = null,
    ServiceLifetime lifetime = ServiceLifetime.Singleton)
  {
    services.TryAddSingleton(TimeProvider.System);
    services.TryAdd(new ServiceDescriptor(
      typeof(IDistributedCache),
      cacheDiKey,
      typeof(CoHostedOrleansCache),
      lifetime));

    return services;
  }

  public static IServiceCollection AddCoHostedOrleansPersistedDistributedCache(
    this IServiceCollection services,
    object? cacheDiKey = null,
    ServiceLifetime lifetime = ServiceLifetime.Singleton)
  {
    services.TryAddSingleton(TimeProvider.System);
    services.TryAdd(new ServiceDescriptor(
      typeof(IDistributedCache),
      cacheDiKey,
      typeof(CoHostedOrleansPersistedCache),
      lifetime));

    return services;
  }

  public static IServiceCollection AddRemoteOrleansDistributedCache(
    this IServiceCollection services,
    object? cacheDiKey = null,
    ServiceLifetime lifetime = ServiceLifetime.Singleton)
  {
    services.TryAdd(new ServiceDescriptor(
      typeof(IDistributedCache),
      cacheDiKey,
      typeof(RemoteOrleansCache),
      lifetime));

    return services;
  }

  public static IServiceCollection AddRemoteOrleansPersistedDistributedCache(
    this IServiceCollection services,
    object? cacheDiKey = null,
    ServiceLifetime lifetime = ServiceLifetime.Singleton)
  {
    services.TryAdd(new ServiceDescriptor(
      typeof(IDistributedCache),
      cacheDiKey,
      typeof(RemoteOrleansPersistedCache),
      lifetime));

    return services;
  }
}

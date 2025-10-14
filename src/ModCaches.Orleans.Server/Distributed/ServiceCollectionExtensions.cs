using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ModCaches.Orleans.Server.Distributed;
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers an IDistributedCache implementation utilizing Microsoft Orleans that keeps data in memory (volatile).
  /// This implementation is intended to be used from within Orleans servers.
  /// </summary>
  /// <param name="services"></param>
  /// <param name="cacheDiKey">Adds as a keyed service if provided.</param>
  /// <param name="lifetime">Service lifetime for cache.</param>
  /// <returns></returns>
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

  /// <summary>
  /// Registers an IDistributedCache implementation utilizing Microsoft Orleans that keeps data in memory and saves them as grain states (persistent).
  /// This implementation is intended to be used from within Orleans servers.
  /// </summary>
  /// <param name="services"></param>
  /// <param name="cacheDiKey">Adds as a keyed service if provided.</param>
  /// <param name="lifetime">Service lifetime for cache.</param>
  /// <returns></returns>
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
}

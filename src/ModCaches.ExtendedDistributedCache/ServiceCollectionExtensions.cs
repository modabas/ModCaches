using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ModCaches.ExtendedDistributedCache;
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers a default <see cref="IExtendedDistributedCache"/> implementation to simplify use of underlying <see cref="IDistributedCache"/>.
  /// </summary>
  /// <param name="services"></param>
  /// <param name="setupAction">Action to configure <see cref="ExtendedDistributedCacheOptions"/>.</param>
  /// <param name="lifetime">Service lifetime for <see cref="DefaultExtendedDistributedCache"/>.</param>
  /// <returns></returns>
  public static IServiceCollection AddExtendedDistributedCache(
    this IServiceCollection services,
    Action<ExtendedDistributedCacheOptions>? setupAction = null,
    ServiceLifetime lifetime = ServiceLifetime.Singleton)
  {
    services.TryAddSingleton<IDistributedCacheSerializer, DefaultDistributedCacheSerializer>();
    Action<ExtendedDistributedCacheOptions> defaultSetupAction = setupAction is null
      ? (options) => options = new ExtendedDistributedCacheOptions()
      : (options) => setupAction(options);
    services.Configure(defaultSetupAction);
    services.TryAdd(new ServiceDescriptor(
      typeof(IExtendedDistributedCache),
      typeof(DefaultExtendedDistributedCache),
      lifetime));
    return services;
  }

  /// <summary>
  /// Registers a default <see cref="IExtendedDistributedCache"/> implementation to simplify use of underlying <see cref="IDistributedCache"/>.
  /// </summary>
  /// <param name="services"></param>
  /// <param name="cacheDiKey">Adds as a keyed service if provided. Also expects an <see cref="IDistributedCache"/> to be registered with same key.</param>
  /// <param name="setupAction">Action to configure <see cref="ExtendedDistributedCacheOptions"/>.</param>
  /// <param name="lifetime">Service lifetime for <see cref="DefaultExtendedDistributedCache"/>.</param>
  /// <returns></returns>
  public static IServiceCollection AddExtendedDistributedCache(
    this IServiceCollection services,
    object? cacheDiKey,
    Action<ExtendedDistributedCacheOptions>? setupAction = null,
    ServiceLifetime lifetime = ServiceLifetime.Singleton)
  {
    services.TryAddKeyedSingleton<IDistributedCacheSerializer, DefaultDistributedCacheSerializer>(cacheDiKey);
    services.TryAdd(new ServiceDescriptor(
      typeof(IExtendedDistributedCache),
      cacheDiKey,
      (sp, key) =>
      {
        var options = new ExtendedDistributedCacheOptions();
        if (setupAction is not null)
        {
          setupAction(options);
        }
        return new DefaultExtendedDistributedCache(
          sp.GetRequiredKeyedService<IDistributedCache>(key),
          Options.Create(options),
          sp.GetRequiredKeyedService<IDistributedCacheSerializer>(key));
      },
      lifetime));

    return services;
  }
}

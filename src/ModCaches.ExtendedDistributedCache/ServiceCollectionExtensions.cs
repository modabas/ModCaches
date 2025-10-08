using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ModCaches.ExtendedDistributedCache;
public static class ServiceCollectionExtensions
{
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

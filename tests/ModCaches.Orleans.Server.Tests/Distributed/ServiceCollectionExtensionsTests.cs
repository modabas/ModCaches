using AwesomeAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using ModCaches.Orleans.Server.Distributed;

namespace ModCaches.Orleans.Server.Tests.Distributed;

public class ServiceCollectionExtensionsTests
{
  [Fact]
  public void AddCoHostedOrleansVolatileDistributedCache_AddsDescriptor_WithDefaultLifetime()
  {
    var services = new ServiceCollection();

    services.AddCoHostedOrleansVolatileDistributedCache();

    var descriptor = services.SingleOrDefault(sd =>
      sd.ServiceType == typeof(IDistributedCache) &&
      sd.ImplementationType == typeof(CoHostedOrleansVolatileCache));

    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
  }

  [Fact]
  public void AddCoHostedOrleansVolatileDistributedCache_WithCustomLifetime_DoesApplyLifetime()
  {
    var services = new ServiceCollection();

    services.AddCoHostedOrleansVolatileDistributedCache(cacheDiKey: new object(), lifetime: ServiceLifetime.Scoped);

    var descriptor = services.SingleOrDefault(sd =>
      sd.ServiceType == typeof(IDistributedCache) &&
      sd.KeyedImplementationType == typeof(CoHostedOrleansVolatileCache));

    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  [Fact]
  public void AddCoHostedOrleansVolatileDistributedCache_TryAdd_PreventsDuplicateRegistrations()
  {
    var services = new ServiceCollection();

    services.AddCoHostedOrleansVolatileDistributedCache();
    services.AddCoHostedOrleansVolatileDistributedCache();

    var descriptors = services.Where(sd =>
      sd.ServiceType == typeof(IDistributedCache) &&
      sd.ImplementationType == typeof(CoHostedOrleansVolatileCache)).ToArray();

    descriptors.Should().HaveCount(1);
  }

  [Fact]
  public void AddCoHostedOrleansPersistentDistributedCache_AddsDescriptor_WithDefaultLifetime()
  {
    var services = new ServiceCollection();

    services.AddCoHostedOrleansPersistentDistributedCache();

    var descriptor = services.SingleOrDefault(sd =>
      sd.ServiceType == typeof(IDistributedCache) &&
      sd.ImplementationType == typeof(CoHostedOrleansPersistentCache));

    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
  }

  [Fact]
  public void AddCoHostedOrleansPersistentDistributedCache_WithCustomLifetime_DoesApplyLifetime()
  {
    var services = new ServiceCollection();

    services.AddCoHostedOrleansPersistentDistributedCache(cacheDiKey: new object(), lifetime: ServiceLifetime.Transient);

    var descriptor = services.SingleOrDefault(sd =>
      sd.ServiceType == typeof(IDistributedCache) &&
      sd.KeyedImplementationType == typeof(CoHostedOrleansPersistentCache));

    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
  }

  [Fact]
  public void AddCoHostedOrleansPersistentDistributedCache_TryAdd_PreventsDuplicateRegistrations()
  {
    var services = new ServiceCollection();

    services.AddCoHostedOrleansPersistentDistributedCache();
    services.AddCoHostedOrleansPersistentDistributedCache();

    var descriptors = services.Where(sd =>
      sd.ServiceType == typeof(IDistributedCache) &&
      sd.ImplementationType == typeof(CoHostedOrleansPersistentCache)).ToArray();

    descriptors.Should().HaveCount(1);
  }
}

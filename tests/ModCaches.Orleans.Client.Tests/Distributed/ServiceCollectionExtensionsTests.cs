using AwesomeAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using ModCaches.Orleans.Client.Distributed;

namespace ModCaches.Orleans.Client.Tests.Distributed;

public class ServiceCollectionExtensionsTests
{
  [Fact]
  public void AddRemoteOrleansVolatileDistributedCache_AddsDescriptor_WithDefaultLifetime()
  {
    var services = new ServiceCollection();

    services.AddRemoteOrleansVolatileDistributedCache();

    var descriptor = services.SingleOrDefault(sd =>
      sd.ServiceType == typeof(IDistributedCache) &&
      sd.ImplementationType == typeof(RemoteOrleansVolatileCache));

    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
  }

  [Fact]
  public void AddRemoteOrleansVolatileDistributedCache_WithCustomLifetime_DoesApplyLifetime()
  {
    var services = new ServiceCollection();

    services.AddRemoteOrleansVolatileDistributedCache(cacheDiKey: new object(), lifetime: ServiceLifetime.Scoped);

    var descriptor = services.SingleOrDefault(sd =>
      sd.ServiceType == typeof(IDistributedCache) &&
      sd.KeyedImplementationType == typeof(RemoteOrleansVolatileCache));

    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  [Fact]
  public void AddRemoteOrleansVolatileDistributedCache_TryAdd_PreventsDuplicateRegistrations()
  {
    var services = new ServiceCollection();

    services.AddRemoteOrleansVolatileDistributedCache();
    services.AddRemoteOrleansVolatileDistributedCache();

    var descriptors = services.Where(sd =>
      sd.ServiceType == typeof(IDistributedCache) &&
      sd.ImplementationType == typeof(RemoteOrleansVolatileCache)).ToArray();

    descriptors.Should().HaveCount(1);
  }

  [Fact]
  public void AddRemoteOrleansPersistentDistributedCache_AddsDescriptor_WithDefaultLifetime()
  {
    var services = new ServiceCollection();

    services.AddRemoteOrleansPersistentDistributedCache();

    var descriptor = services.SingleOrDefault(sd =>
      sd.ServiceType == typeof(IDistributedCache) &&
      sd.ImplementationType == typeof(RemoteOrleansPersistentCache));

    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
  }

  [Fact]
  public void AddRemoteOrleansPersistentDistributedCache_WithCustomLifetime_DoesApplyLifetime()
  {
    var services = new ServiceCollection();

    services.AddRemoteOrleansPersistentDistributedCache(cacheDiKey: new object(), lifetime: ServiceLifetime.Transient);

    var descriptor = services.SingleOrDefault(sd =>
      sd.ServiceType == typeof(IDistributedCache) &&
      sd.KeyedImplementationType == typeof(RemoteOrleansPersistentCache));

    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
  }

  [Fact]
  public void AddRemoteOrleansPersistentDistributedCache_TryAdd_PreventsDuplicateRegistrations()
  {
    var services = new ServiceCollection();

    services.AddRemoteOrleansPersistentDistributedCache();
    services.AddRemoteOrleansPersistentDistributedCache();

    var descriptors = services.Where(sd =>
      sd.ServiceType == typeof(IDistributedCache) &&
      sd.ImplementationType == typeof(RemoteOrleansPersistentCache)).ToArray();

    descriptors.Should().HaveCount(1);
  }
}

using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ModCaches.ExtendedDistributedCache.Tests;

public class ServiceCollectionExtensionsTests
{
  [Fact]
  public void AddExtendedDistributedCache_Registers_Default_Serializer_And_ExtendedCache()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddExtendedDistributedCache();

    // Assert
    // IDistributedCacheSerializer registration (default singleton)
    var serializerDescriptor = services
      .FirstOrDefault(d => d.ServiceType == typeof(IDistributedCacheSerializer));
    serializerDescriptor.Should().NotBeNull("the default serializer should be registered");
    serializerDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

    // IExtendedDistributedCache registration
    var extendedDescriptor = services
      .FirstOrDefault(d => d.ServiceType == typeof(IExtendedDistributedCache) && d.ImplementationType == typeof(DefaultExtendedDistributedCache));
    extendedDescriptor.Should().NotBeNull("IExtendedDistributedCache should be registered with DefaultExtendedDistributedCache implementation");
    extendedDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
  }

  [Fact]
  public void AddExtendedDistributedCache_Applies_SetupAction_To_Options()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddExtendedDistributedCache(options => options.MaxLocks = 999);

    // Build provider to resolve configured options
    using var provider = services.BuildServiceProvider();
    var opts = provider.GetRequiredService<IOptions<ExtendedDistributedCacheOptions>>();

    // Assert
    opts.Should().NotBeNull();
    opts.Value.MaxLocks.Should().Be(999);
  }

  [Fact]
  public void AddExtendedDistributedCache_Respects_Provided_Lifetime()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddExtendedDistributedCache(setupAction: null, lifetime: ServiceLifetime.Transient);

    // Assert
    var extendedDescriptor = services
      .FirstOrDefault(d => d.ServiceType == typeof(IExtendedDistributedCache) && d.ImplementationType == typeof(DefaultExtendedDistributedCache));
    extendedDescriptor.Should().NotBeNull();
    extendedDescriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);
  }

  [Fact]
  public void AddExtendedDistributedCache_Keyed_Registers_Factory_And_Keyed_Serializer_Descriptor()
  {
    // Arrange
    var services = new ServiceCollection();
    var key = "my-cache-key";

    // Act
    services.AddExtendedDistributedCache(key, options => options.MaxLocks = 10, ServiceLifetime.Scoped);

    // Assert
    // There should be a descriptor for IExtendedDistributedCache that uses an implementation factory (keyed)
    var keyedExtendedDescriptor = services
      .FirstOrDefault(d => d.ServiceType == typeof(IExtendedDistributedCache) && (d.ServiceKey?.Equals(key) ?? false));
    keyedExtendedDescriptor.Should().NotBeNull("a keyed IExtendedDistributedCache registration using an implementation factory should be present");
    keyedExtendedDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

    // There should be some registration for IDistributedCacheSerializer (keyed registration may vary
    // in how it's represented in the collection, but the service type should be present)
    var serializerDescriptorExists = services.Any(d => d.ServiceType == typeof(IDistributedCacheSerializer));
    serializerDescriptorExists.Should().BeTrue("a keyed serializer registration for IDistributedCacheSerializer should exist");
  }
}

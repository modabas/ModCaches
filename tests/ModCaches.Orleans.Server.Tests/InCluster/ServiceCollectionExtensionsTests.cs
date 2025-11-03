using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModCaches.Orleans.Server.InCluster;

namespace ModCaches.Orleans.Server.Tests.InCluster;

public class ServiceCollectionExtensionsTests
{
  [Fact]
  public void AddOrleansInClusterCache_Registers_TimeProvider_System()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddOrleansInClusterCache();
    using var provider = services.BuildServiceProvider();
    var registered = provider.GetRequiredService<TimeProvider>();

    // Assert
    registered.Should().BeSameAs(TimeProvider.System);
  }

  [Fact]
  public void AddOrleansInClusterCache_Default_Does_Not_Modify_Options()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddOrleansInClusterCache();
    using var provider = services.BuildServiceProvider();
    var options = provider.GetRequiredService<IOptions<CacheGrainEntryOptions>>().Value;

    // Assert - default lambda in the production code does not mutate the options instance,
    // so all properties should remain null.
    options.AbsoluteExpiration.Should().BeNull();
    options.AbsoluteExpirationRelativeToNow.Should().BeNull();
    options.SlidingExpiration.Should().BeNull();
  }

  [Fact]
  public void AddOrleansInClusterCache_Applies_SetupAction_To_Options()
  {
    // Arrange
    var services = new ServiceCollection();
    var expectedAbsRelToNow = TimeSpan.FromMinutes(5);
    var expectedSliding = TimeSpan.FromSeconds(10);
    var expectedAbs = DateTimeOffset.UtcNow.AddMinutes(30);

    // Act
    services.AddOrleansInClusterCache(options =>
    {
      options.AbsoluteExpirationRelativeToNow = expectedAbsRelToNow;
      options.SlidingExpiration = expectedSliding;
      options.AbsoluteExpiration = expectedAbs;
    });

    using var provider = services.BuildServiceProvider();
    var options = provider.GetRequiredService<IOptions<CacheGrainEntryOptions>>().Value;

    // Assert
    options.AbsoluteExpirationRelativeToNow.Should().Be(expectedAbsRelToNow);
    options.SlidingExpiration.Should().Be(expectedSliding);
    options.AbsoluteExpiration.Should().Be(expectedAbs);
  }
}

using System.Text;
using System.Text.Json;
using AwesomeAssertions;

namespace ModCaches.ExtendedDistributedCache.Tests;

public class DefaultDistributedCacheSerializerTests
{
  private readonly DefaultDistributedCacheSerializer _serializer = new();

  private record Person(string Name, int Age);

  [Fact]
  public async Task SerializeAsync_Primitive_ReturnsExpectedUtf8BytesAsync()
  {
    var result = await _serializer.SerializeAsync<int>(42, CancellationToken.None);
    var expected = JsonSerializer.SerializeToUtf8Bytes(42);

    result.ToArray().Should().Equal(expected);
    Encoding.UTF8.GetString(result.Span).Should().Be("42");
  }

  [Fact]
  public async Task SerializeAsync_Object_ReturnsExpectedUtf8BytesAsync()
  {
    var person = new Person("Alice", 30);
    var result = await _serializer.SerializeAsync(person, CancellationToken.None);
    var expected = JsonSerializer.SerializeToUtf8Bytes(person);

    result.ToArray().Should().Equal(expected);
    var deserialized = JsonSerializer.Deserialize<Person>(result.Span);
    deserialized.Should().BeEquivalentTo(person);
  }

  [Fact]
  public async Task SerializeAsync_NullReferenceType_ReturnsJsonNullAsync()
  {
    var result = await _serializer.SerializeAsync<string?>(null, CancellationToken.None);
    var expected = JsonSerializer.SerializeToUtf8Bytes<string?>(null);

    result.ToArray().Should().Equal(expected);
    Encoding.UTF8.GetString(result.Span).Should().Be("null");
  }

  [Fact]
  public async Task DeserializeAsync_EmptyBytes_ReturnsDefaultForReferenceTypeAsync()
  {
    var result = await _serializer.DeserializeAsync<string>(Memory<byte>.Empty, CancellationToken.None);
    result.Should().BeNull();
  }

  [Fact]
  public async Task DeserializeAsync_EmptyBytes_ReturnsDefaultForValueTypeAsync()
  {
    var result = await _serializer.DeserializeAsync<int>(Memory<byte>.Empty, CancellationToken.None);
    result.Should().Be(0);
  }

  [Fact]
  public async Task DeserializeAsync_ValidJson_DeserializesToObjectAsync()
  {
    var person = new Person("Bob", 45);
    var bytes = JsonSerializer.SerializeToUtf8Bytes(person);
    var result = await _serializer.DeserializeAsync<Person>(new Memory<byte>(bytes), CancellationToken.None);

    result.Should().BeEquivalentTo(person);
  }

  [Fact]
  public async Task Methods_DoNotThrow_WhenCancellationTokenIsCanceledAsync()
  {
    using var cts = new CancellationTokenSource();
    cts.Cancel(); // canceled token

    var serialized = await _serializer.SerializeAsync("x", cts.Token);
    serialized.Length.Should().BeGreaterThan(0);

    var deserialized = await _serializer.DeserializeAsync<string>(serialized, cts.Token);
    deserialized.Should().Be("x");
  }
}

using System.Collections.Concurrent;
using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace ModCaches.ExtendedDistributedCache.Tests;

public class DefaultExtendedDistributedCacheTests
{
  private readonly ExtendedDistributedCacheOptions _defaultOptions = new()
  {
    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(1),
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
    SlidingExpiration = TimeSpan.FromMinutes(30),
    MaxLocks = 16
  };

  [Fact]
  public void DistributedCache_Property_ExposesInnerCache()
  {
    var mem = new InMemoryDistributedCache();
    var svc = CreateCache(mem, new JsonSerializer());
    svc.DistributedCache.Should().BeSameAs(mem);
  }

  [Fact]
  public async Task GetOrCreateAsync_WhenCacheHit_DeserializesAndReturnsValueAsync()
  {
    var mem = new InMemoryDistributedCache();
    var serializer = new JsonSerializer();
    var svc = CreateCache(mem, serializer);

    var key = "k1";
    var original = 12345;
    var bytes = await serializer.SerializeAsync(original, CancellationToken.None);
    await mem.SetAsync(key, bytes.ToArray(), new DistributedCacheEntryOptions(), CancellationToken.None);

    // factory should not be called
    Func<CancellationToken, Task<int>> factory = _ => throw new InvalidOperationException("Factory invoked");

    var result = await svc.GetOrCreateAsync<int>(key, factory, CancellationToken.None);
    result.Should().Be(original);
  }

  [Fact]
  public async Task GetOrCreateAsync_WhenCacheMiss_CallsFactoryAndSetsCacheAsync()
  {
    var mem = new InMemoryDistributedCache();
    var serializer = new JsonSerializer();
    var svc = CreateCache(mem, serializer);

    var key = "k-miss";
    var value = "hello world";

    var result = await svc.GetOrCreateAsync<string>(key, _ => Task.FromResult(value), CancellationToken.None);
    result.Should().Be(value);

    // verify cache was set
    var stored = await mem.GetAsync(key, CancellationToken.None);
    stored.Should().NotBeNull();
    var deserialized = await serializer.DeserializeAsync<string>(stored, CancellationToken.None);
    deserialized.Should().Be(value);
  }

  [Fact]
  public async Task GetOrCreateAsync_ConcurrentCalls_OnlyInvokesFactoryOnceAsync()
  {
    var mem = new InMemoryDistributedCache();
    var serializer = new JsonSerializer();
    var svc = CreateCache(mem, serializer);

    var key = "concurrent";
    int calls = 0;

    Func<CancellationToken, Task<int>> factory = async ct =>
    {
      Interlocked.Increment(ref calls);
      // simulate expensive work
      await Task.Delay(100, ct);
      return 42;
    };

    var t1 = svc.GetOrCreateAsync<int>(key, factory, CancellationToken.None);
    var t2 = svc.GetOrCreateAsync<int>(key, factory, CancellationToken.None);

    var results = await Task.WhenAll(t1, t2);
    results[0].Should().Be(42);
    results[1].Should().Be(42);
    calls.Should().Be(1);
  }

  [Fact]
  public async Task GetOrCreateAsync_WhenDeserializedNull_ThrowsInvalidOperationExceptionAsync()
  {
    var mem = new InMemoryDistributedCache();
    var serializer = new NullDeserializerSerializer(); // Deserialize returns null
    var svc = CreateCache(mem, serializer);

    var key = "bad";
    // put some bytes so flow goes to DeserializeAsync
    await mem.SetAsync(key, Encoding.UTF8.GetBytes("ignored"), new DistributedCacheEntryOptions(), CancellationToken.None);

    Func<CancellationToken, Task<string>> factory = _ => Task.FromResult("should-not-be-used");

    await svc.Invoking(s => s.GetOrCreateAsync<string>(key, factory, CancellationToken.None))
        .Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Deserialized value is null.");
  }

  // New tests for the TState overload of GetOrCreateAsync<TState, T>

  [Fact]
  public async Task GetOrCreateAsync_TState_WhenCacheHit_DeserializesAndReturnsValueAsync()
  {
    var mem = new InMemoryDistributedCache();
    var serializer = new JsonSerializer();
    var svc = CreateCache(mem, serializer);

    var key = "k1-state";
    var original = 12345;
    var bytes = await serializer.SerializeAsync(original, CancellationToken.None);
    await mem.SetAsync(key, bytes.ToArray(), new DistributedCacheEntryOptions(), CancellationToken.None);

    // factory should not be called
    Func<string, CancellationToken, Task<int>> factory = (_, ct) => throw new InvalidOperationException("Factory invoked");

    var result = await svc.GetOrCreateAsync<string, int>(key, "ignored-state", factory, CancellationToken.None);
    result.Should().Be(original);
  }

  [Fact]
  public async Task GetOrCreateAsync_TState_WhenCacheMiss_CallsFactoryAndSetsCacheAsync()
  {
    var mem = new InMemoryDistributedCache();
    var serializer = new JsonSerializer();
    var svc = CreateCache(mem, serializer);

    var key = "k-miss-state";
    var state = "state-";
    Func<string, CancellationToken, Task<string>> factory = (s, ct) => Task.FromResult(s + "hello");

    var result = await svc.GetOrCreateAsync<string, string>(key, state, factory, CancellationToken.None);
    result.Should().Be(state + "hello");

    // verify cache was set
    var stored = await mem.GetAsync(key, CancellationToken.None);
    stored.Should().NotBeNull();
    var deserialized = await serializer.DeserializeAsync<string>(stored, CancellationToken.None);
    deserialized.Should().Be(state + "hello");
  }

  [Fact]
  public async Task GetOrCreateAsync_TState_ConcurrentCalls_OnlyInvokesFactoryOnceAsync()
  {
    var mem = new InMemoryDistributedCache();
    var serializer = new JsonSerializer();
    var svc = CreateCache(mem, serializer);

    var key = "concurrent-state";
    int calls = 0;

    Func<string, CancellationToken, Task<int>> factory = async (s, ct) =>
    {
      Interlocked.Increment(ref calls);
      // simulate expensive work
      await Task.Delay(100, ct);
      return 42;
    };

    var t1 = svc.GetOrCreateAsync<string, int>(key, "state", factory, CancellationToken.None);
    var t2 = svc.GetOrCreateAsync<string, int>(key, "state", factory, CancellationToken.None);

    var results = await Task.WhenAll(t1, t2);
    results[0].Should().Be(42);
    results[1].Should().Be(42);
    calls.Should().Be(1);
  }

  [Fact]
  public async Task GetOrCreateAsync_TState_WhenDeserializedNull_ThrowsInvalidOperationExceptionAsync()
  {
    var mem = new InMemoryDistributedCache();
    var serializer = new NullDeserializerSerializer();
    var svc = CreateCache(mem, serializer);

    var key = "bad-state";
    await mem.SetAsync(key, Encoding.UTF8.GetBytes("ignored"), new DistributedCacheEntryOptions(), CancellationToken.None);

    Func<string, CancellationToken, Task<string>> factory = (s, ct) => Task.FromResult(s + "-should-not-be-used");

    await svc.Invoking(s => s.GetOrCreateAsync<string, string>(key, "st", factory, CancellationToken.None))
        .Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Deserialized value is null.");
  }

  [Fact]
  public async Task SetAsync_UsesProvidedOptions_WhenOptionsPassedAsync()
  {
    var mem = new InMemoryDistributedCache();
    var serializer = new JsonSerializer();
    var svc = CreateCache(mem, serializer);

    var key = "setopts";
    var value = 10;
    var opts = new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
      SlidingExpiration = TimeSpan.FromMinutes(1)
    };

    await svc.SetAsync(key, value, CancellationToken.None, opts);

    mem.CapturedOptions.TryGetValue(key, out var captured).Should().BeTrue();
    captured.Should().NotBeNull();
    captured.AbsoluteExpirationRelativeToNow.Should().Be(opts.AbsoluteExpirationRelativeToNow);
    captured.SlidingExpiration.Should().Be(opts.SlidingExpiration);

    // Ensure value was serialized and stored
    var bytes = await mem.GetAsync(key, CancellationToken.None);
    var deserialized = await serializer.DeserializeAsync<int>(bytes, CancellationToken.None);
    deserialized.Should().Be(value);
  }

  [Fact]
  public async Task SetAsync_UsesDefaultOptions_WhenOptionsNullAsync()
  {
    var mem = new InMemoryDistributedCache();
    var serializer = new JsonSerializer();
    var options = Options.Create(new ExtendedDistributedCacheOptions
    {
      AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(20),
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20),
      SlidingExpiration = TimeSpan.FromMinutes(10)
    });

    var svc = new DefaultExtendedDistributedCache(mem, options, serializer);

    var key = "setdefault";
    var value = 77;

    await svc.SetAsync(key, value, CancellationToken.None, options: null);

    mem.CapturedOptions.TryGetValue(key, out var captured).Should().BeTrue();
    captured.Should().NotBeNull();
    captured.AbsoluteExpiration.Should().Be(options.Value.AbsoluteExpiration);
    captured.AbsoluteExpirationRelativeToNow.Should().Be(options.Value.AbsoluteExpirationRelativeToNow);
    captured.SlidingExpiration.Should().Be(options.Value.SlidingExpiration);
  }

  [Fact]
  public async Task TryGetValueAsync_WhenCacheHit_ReturnsTrueAndValueAsync()
  {
    var mem = new InMemoryDistributedCache();
    var serializer = new JsonSerializer();
    var svc = CreateCache(mem, serializer);

    var key = "try-get";
    var value = Guid.NewGuid();
    var bytes = await serializer.SerializeAsync(value, CancellationToken.None);
    await mem.SetAsync(key, bytes.ToArray(), new DistributedCacheEntryOptions(), CancellationToken.None);

    var (ok, v) = await svc.TryGetValueAsync<Guid>(key, CancellationToken.None);
    ok.Should().BeTrue();
    v.Should().Be(value);
  }

  [Fact]
  public async Task TryGetValueAsync_WhenCacheMiss_ReturnsFalseAndDefaultAsync()
  {
    var mem = new InMemoryDistributedCache();
    var serializer = new JsonSerializer();
    var svc = CreateCache(mem, serializer);

    var (ok, v) = await svc.TryGetValueAsync<int>("not-here", CancellationToken.None);
    ok.Should().BeFalse();
    v.Should().Be(default);
  }

  [Fact]
  public async Task TryGetValueAsync_WhenDeserializeNull_ThrowsInvalidOperationExceptionAsync()
  {
    var mem = new InMemoryDistributedCache();
    var serializer = new NullDeserializerSerializer();
    var svc = CreateCache(mem, serializer);

    var key = "bad2";
    await mem.SetAsync(key, Encoding.UTF8.GetBytes("ignored"), new DistributedCacheEntryOptions(), CancellationToken.None);

    await svc.Invoking(s => s.TryGetValueAsync<string>(key, CancellationToken.None))
        .Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Deserialized value is null.");
  }

  // Helpers

  private static DefaultExtendedDistributedCache CreateCache(IDistributedCache cache, IDistributedCacheSerializer serializer)
  {
    var options = Options.Create(new ExtendedDistributedCacheOptions
    {
      AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(5),
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
      SlidingExpiration = TimeSpan.FromMinutes(2),
      MaxLocks = 16
    });

    return new DefaultExtendedDistributedCache(cache, options, serializer);
  }

  // Simple in-memory implementation of IDistributedCache for tests. Captures options passed to SetAsync.
  private class InMemoryDistributedCache : IDistributedCache
  {
    private readonly ConcurrentDictionary<string, byte[]> _store = new();
    public ConcurrentDictionary<string, DistributedCacheEntryOptions> CapturedOptions { get; } = new();

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
      _store.TryGetValue(key, out var bytes);
      return Task.FromResult<byte[]?>(bytes);
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
      _store[key] = value;
      CapturedOptions[key] = options ?? new DistributedCacheEntryOptions();
      return Task.CompletedTask;
    }

    public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;
    public Task RemoveAsync(string key, CancellationToken token = default)
    {
      _store.TryRemove(key, out _);
      CapturedOptions.TryRemove(key, out _);
      return Task.CompletedTask;
    }

    // Synchronous helpers used by tests if needed
    public byte[]? GetSync(string key) => _store.TryGetValue(key, out var b) ? b : null;

    public byte[]? Get(string key)
    {
      throw new NotImplementedException();
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
      throw new NotImplementedException();
    }

    public void Refresh(string key)
    {
      throw new NotImplementedException();
    }

    public void Remove(string key)
    {
      throw new NotImplementedException();
    }
  }

  // Serializer using System.Text.Json
  private class JsonSerializer : IDistributedCacheSerializer
  {
    public ValueTask<Memory<byte>> SerializeAsync<T>(T value, CancellationToken ct)
    {
      var data = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);
      return ValueTask.FromResult<Memory<byte>>(new Memory<byte>(data));
    }

    public ValueTask<T?> DeserializeAsync<T>(Memory<byte> bytes, CancellationToken ct)
    {
      if (bytes.IsEmpty) return ValueTask.FromResult<T?>(default);
      var value = System.Text.Json.JsonSerializer.Deserialize<T>(bytes.Span);
      return ValueTask.FromResult(value);
    }
  }

  // Serializer that always returns null on deserialize (to trigger InvalidOperationException)
  private class NullDeserializerSerializer : IDistributedCacheSerializer
  {
    public ValueTask<Memory<byte>> SerializeAsync<T>(T value, CancellationToken ct)
    {
      var data = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);
      return ValueTask.FromResult<Memory<byte>>(new Memory<byte>(data));
    }

    public ValueTask<T?> DeserializeAsync<T>(Memory<byte> bytes, CancellationToken ct)
    {
      return ValueTask.FromResult<T?>(default);
    }
  }
}

# Extended Distributed Cache

Simplifies the usage of any distributed cache that implements the `IDistributedCache` interface.

## ✨ Features

- Built-in serializer support to simplify distributed-cache usage
- In-process cache stampede protection
- HybridCache-like interface that simplifies storing and fetching data from any `IDistributedCache` implementation

> **Note**: Cache stampede protection is achieved via an in-memory least-recently-used (LRU) cache implementation from the [Microsoft Orleans](https://github.com/dotnet/orleans) project.

## 🛠️ Getting Started

### Install the NuGet Package:

```bash
dotnet add package ModCaches.ExtendedDistributedCache
```

### Register Services:

In your `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExtendedDistributedCache();
```

or, if the `IDistributedCache` service is registered as a keyed service:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExtendedDistributedCache(serviceKey);
```

> **Note**: These methods can also be used to configure the default `DistributedCacheEntryOptions` via the optional `Action<ExtendedDistributedCacheOptions>? setupAction` parameter.


### Resolve from Dependency Injection

Resolve `IExtendedDistributedCache` via dependency injection (for example, constructor injection) to start using it.

```csharp
public class MyService(IExtendedDistributedCache cache) 
{
}
```

## 🧩 How To Use

The `IExtendedDistributedCache` exposes several methods to interact with:

- `GetOrCreateAsync` method — fetch a cached value, or create one using the supplied factory if the cached value is not found
- `SetAsync` method — store a value in the cache
- `TryGetAsync` method — fetch a cached value if it exists
- `DistributedCache` property — access the underlying `IDistributedCache`

A serializer converts values of type `T` to the `byte[]` that `IDistributedCache` methods expect, and back from `byte[]` to `T`. The default serializer provided by the package uses `System.Text.Json`.

> **Note**: If the optional `DistributedCacheEntryOptions` parameter is not provided for a method, the default value configured during service registration will be used.

``` csharp
  IDistributedCache DistributedCache { get; }

  Task<T> GetOrCreateAsync<T>(
    string key,
    Func<CancellationToken, Task<T>> factory,
    CancellationToken ct,
    DistributedCacheEntryOptions? options = null);

  Task<T> GetOrCreateAsync<TState, T>(
    string key,
    TState state, 
    Func<TState, CancellationToken, Task<T>> factory,
    CancellationToken ct,
    DistributedCacheEntryOptions? options = null);

  Task SetAsync<T>(
    string key,
    T value,
    CancellationToken ct,
    DistributedCacheEntryOptions? options = null);

  Task<(bool, T?)> TryGetValueAsync<T>(string key, CancellationToken ct);
```

### Get or Create a Cached Value

To get a value from distributed cache simply use GetOrCreate method and provide the cache key.

- If cached value exists in distributed cache, it will be fetched and deserialized to type T,
- If cached value does not exist, factory method defined in `Func<CancellationToken, Task<T>> factory` parameter will be invoked. Resulting value will be stored in distributed cache and then returned to caller.

``` csharp
    var forecast = await cache.GetOrCreateAsync(
      "weatherforecast",
      async (ct) =>
      {
        // Simulate a long-running operation
        await Task.Delay(5000, ct);
        return Enumerable.Range(1, 5).Select(index =>
          new WeatherForecastDistributedCacheItem
          (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            _summaries[Random.Shared.Next(_summaries.Length)]
          )).ToArray();
      },
      ct: ct);
```
# Extended Distributed Cache

Simplifies usage of any distributed cache that implements IDistributedCache interface.

## ✨ Features

- Built-in serializer support to simplify distributed cache usage,
- In process cache stampede protection,
- HybridCache-like interface simplifying storing and fetching data from any IDistributedCache implementation,

> **Note**: Cache stampede protection is achieved via an in memory least recently used cache implementation from [Microsoft Orleans]() project.

## 🛠️ Getting Started

### Install the NuGet Package:

```bash
dotnet add package ModCaches.ExtendedDistributedCache
```

### Register Services:

In your `Program.cs`:

``` csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExtendedDistributedCache();
```

or if IDistributedCache service in use is registered as a keyed service:

``` csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExtendedDistributedCache(serviceKey);
```

> **Note**: These methods can also be used to configure default DistributedCacheEntryOptions value via the optional `Action<ExtendedDistributedCacheOptions>? setupAction` parameter.


### Resolve from Dependency Injection

Simply resolve IExtendedDistributedCache from dependency injection (i.e. constructor dependency injection) in application code to start using.

``` csharp
public class MyService(IExtendedDistributedCache cache) 
{
}
```


## 🧩 How To Use

IExtendedDistributedCache exposes a couple of methods to interact with:

- GetOrCreate method to fetch a cached value or create one via factory input parameter if cached value is not found,
- Set method to store a value in cache,
- TryGet method to fetch cached value if exists,
- DistributedCache property to access underlying distributed cache

A serializer is used to convert value of type T to the byte[] IDistributedCache methods expect and also convert back from byte[] to type T. Default serializer provided in the package uses System.Text.Json.

> **Note**: If optional DistributedCacheEntryOptions parameter is not provided for any of these methods, default value configured during application service registration will be used.

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

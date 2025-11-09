# Microsoft Orleans In-Cluster Cache

Abstract Microsoft Orleans grain implementations to cache data in memory (volatile) or can also save as grain state (persistent).

## ✨ Features

- Utilizes Orleans' built in features like request scheduling for cache stampede protection and serialization for passing cache data around,
- Simplifies architecture of a Microsoft Orleans project that needs a caching layer by providing it within Orleans itself, eliminating the need for a seperate caching server,
- Supports Cache-Aside, Read-Through, Write-Around and Write-Through caching strategies.

## 🛠️ Getting Started

### Install the NuGet Package:

```bash
dotnet add package ModCaches.Orleans.Server
```

### Register Services:

In your `Program.cs`:

``` csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOrleansInClusterCache(options =>
{
  options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
  options.SlidingExpiration = TimeSpan.FromMinutes(1);
});
```

> **Note**: `Action<InClusterCacheOptions>? setupAction` parameter is optional and is used to configure default options.

## 🧩 Implementation

Implementing in-cluster cache grains is straightforward. However, it's important to choose the appropriate caching strategy based on your application's requirements. The available strategies are:
- **Cache-Aside**: The application code is responsible for checking the cache before fetching data from the source. If the data is not in the cache, it fetches it from the source and then stores it in the cache.
- **Read-Through**: The cache itself is responsible for fetching data from the source when a cache miss occurs. The application code simply requests data from the cache, and the cache handles the retrieval and storage.
- **Write-Around**: The application code writes data directly to the source, bypassing the cache. The cache is only updated when data is read.
- **Write-Through**: The application code writes data to both the cache and the source simultaneously, ensuring that the cache is always up-to-date.


To cache a value using in-cluster cache grains, start by creating a marker interface inheriting one or more of the following interfaces based on the caching strategy you want to use:
- `ICacheGrain<TValue>` for Cache-Aside and Write-Around strategy,
- `IReadThroughCacheGrain<TValue>` or `IReadThroughCacheGrain<TValue, TCreateArgs>` for Read-Through strategy, (includes `ICacheGrain<TValue>`)
- `IWriteThroughCacheGrain<TValue>` for Write-Through strategy, (includes `ICacheGrain<TValue>`)

Then create a cache grain implementation inheriting one of the abstract base cache grain types and your marker interface:

- `VolatileCacheGrain<TValue>` or `VolatileCacheGrain<TValue, TCreateArgs>` for storing data in memory,
- `PersistentCacheGrain<TValue>` or `PersistentCacheGrain<TValue, TCreateArgs>` for also persisting it as grain state,

> **Note**: Read-Through cache grains require implementation of `ReadThroughAsync` method to create cache entries when not found/expired. Write-Through cache grains require implementation of `WriteThroughAsync` method to handle write operations. Default implementations of these methods throw `NotImplementedException`.

Sample below implements read-through cache pattern, by inheriting `VolatileCacheGrain<TValue, TCreateArgs>` and by using marker interface `IWeatherForecastCacheGrain`:
``` csharp
//marker interface
internal interface IWeatherForecastCacheGrain : IReadThroughCacheGrain<WeatherForecastCacheValue, WeatherForecastCacheArgs>;

//grain implementation
internal class WeatherForecastCacheGrain :
  VolatileCacheGrain<WeatherForecastCacheValue, WeatherForecastCacheArgs>,
  IWeatherForecastCacheGrain
{
  private static readonly string[] _summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

  public WeatherForecastCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  protected override async Task<ReadThroughResult<WeatherForecastCacheValue>> ReadThroughAsync(
    WeatherForecastCacheArgs? args,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    var dayCount = args?.DayCount ?? 5;
    // Simulate a long-running operation
    await Task.Delay(5000, ct);
    var value = new WeatherForecastCacheValue()
    {
      Items = Enumerable.Range(1, dayCount).Select(index => new WeatherForecastCacheValueItem()
      {
        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = _summaries[Random.Shared.Next(_summaries.Length)]
      }).ToArray()
    };
    return new ReadThroughResult<WeatherForecastCacheValue>(Value: value, Options: options);
  }
}

[GenerateSerializer]
internal struct WeatherForecastCacheValue
{
  [Id(0)]
  public WeatherForecastCacheValueItem[] Items { get; init; }
}

[GenerateSerializer]
internal struct WeatherForecastCacheValueItem
{
  [Id(0)]
  public DateOnly Date { get; init; }
  [Id(1)]
  public int TemperatureC { get; init; }
  [Id(2)]
  public string? Summary { get; init; }
}

[GenerateSerializer]
internal record WeatherForecastCacheArgs(int DayCount);
```

To utilize write-through cache pattern, add necessary `IWriteThroughCacheGrain<TValue>` interface to marker interface, then override and implement `WriteThroughAsync` method in cache grain class:
``` csharp
//marker interface
internal interface IWeatherForecastCacheGrain : 
    IReadThroughCacheGrain<WeatherForecastCacheValue, WeatherForecastCacheArgs>,
    IWriteThroughCacheGrain<WeatherForecastCacheValue>;

//grain implementation
internal class WeatherForecastCacheGrain :
  VolatileCacheGrain<WeatherForecastCacheValue, WeatherForecastCacheArgs>,
  IWeatherForecastCacheGrain
{
  private static readonly string[] _summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

  public WeatherForecastCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  protected override async Task<WriteThroughResult<WeatherForecastCacheValue>> WriteThroughAsync(
    WeatherForecastCacheValue value,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    // Write to an external data source
    // e.g., await _database.SaveAsync(value, ct);
    
    return new WriteThroughResult<WeatherForecastCacheValue>(Value: value, Options: options);
  }

  protected override async Task<ReadThroughResult<WeatherForecastCacheValue>> ReadThroughAsync(
    WeatherForecastCacheArgs? args,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    var dayCount = args?.DayCount ?? 5;
    // Simulate a long-running operation
    await Task.Delay(5000, ct);
    var value = new WeatherForecastCacheValue()
    {
      Items = Enumerable.Range(1, dayCount).Select(index => new WeatherForecastCacheValueItem()
      {
        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = _summaries[Random.Shared.Next(_summaries.Length)]
      }).ToArray()
    };
    return new ReadThroughResult<WeatherForecastCacheValue>(Value: value, Options: options);
  }
}
```


Creating similar cache grain but with state-persistence requires implementation of `PersistentCacheGrain<TValue, TCreateArgs>` instead:

> **Note**: Persistent cache requires a configured grain storage on Microsoft Orleans server.

``` csharp
//marker interface (same as original)
internal interface IWeatherForecastCacheGrain : IReadThroughCacheGrain<WeatherForecastCacheValue, WeatherForecastCacheArgs>;

//grain implementation
internal class WeatherForecastCacheGrain :
  PersistentCacheGrain<WeatherForecastCacheValue, WeatherForecastCacheArgs>,
  IWeatherForecastCacheGrain
{
  private static readonly string[] _summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

  public WeatherForecastCacheGrain(
    IServiceProvider serviceProvider, 
    [PersistentState(nameof(WeatherForecastCacheGrain))]IPersistentState<CacheState<WeatherForecastCacheValue>> persistentState)
    : base(serviceProvider, persistentState)
  {
  }

  protected override async Task<ReadThroughResult<WeatherForecastCacheValue>> ReadThroughAsync(
    WeatherForecastCacheArgs? args,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    //Create cache entry
    //Invoked when GetOrCreate or Create methods are called
    ///...
  }
}
```

## 🧩 How To Call Implemented Cache Grains

In-Cluster cache grains expose a couple of methods to interact with:

- `ICacheGrain<TValue>` methods:
    1. `SetAsync` method — store a value in cache and return stored value,
    2. `TryGetAsync` method — fetch unexpired cached value if exists (updating last accessed time used for sliding expiration),
    3. `TryPeekAsync` method — fetch unexpired cached value if exists (without updating last accessed time used for sliding expiration),
    4. `RefreshAsync` method — update last accessed time used for sliding expiration if an unexpired cache value exists,
    5. `RemoveAsync` method — clear cache value
- `IReadThroughCacheGrain<TValue>` and `IReadThroughCacheGrain<TValue, TCreateArgs>` methods:
    1. `GetOrCreateAsync` method — fetch a cached value or create one via underlying `ReadThroughAsync` method if cached value is not found/has expired,
    2. `CreateAsync` method — create a cache value via `ReadThroughAsync` method and fetch it,
    3. Methods inherited from `ICacheGrain<TValue>`
- `IWriteThroughCacheGrain<TValue>` methods:
    1. `SetAndWriteAsync` method — write value via underlying `WriteThroughAsync` method and update cache with the written value.
    2. Methods inherited from `ICacheGrain<TValue>`

So calling the read-through `WeatherForecastCacheGrain` sample implemented above to get cache value would be:

``` csharp
var args = new WeatherForecastCacheArgs(7);

await grainFactory.GetGrain<IWeatherForecastCacheGrain>("weatherforecast").GetOrCreateAsync(args, ct);
```

...where grainFactory is a Microsoft.Orleans `IGrainFactory` instance.

### Overriding cache options input parameter during read-through operation

For a read-through cache, if you need to adjust caching options based on the generated value within the `ReadThroughAsync` method (for example, when the value includes a token with its own lifetime), simply return the updated options along with the value from `ReadThroughAsync`:

``` csharp
  protected override async Task<ReadThroughResult<WeatherForecastCacheValue>> ReadThroughAsync(
    WeatherForecastCacheArgs? args,
    CacheGrainEntryOptions options,
    CancellationToken ct)
{
    //generate cache entry value, e.g. read from external service
    // var value = ...

    //modify options as needed
    return new ReadThroughResult<WeatherForecastCacheValue>(
        Value: value, 
        Options: new CacheGrainEntryOptions(
            AbsoluteExpiration: default,
            AbsoluteExpirationRelativeToNow: TimeSpan.FromMinutes(5),
            SlidingExpiration: TimeSpan.FromMinutes(2)));
}
```

### Overriding value and cache options input parameters during write-through operation

Similarly, for a write-through cache, it's possible to modify input value and options when setting a cache value via `SetAndWriteAsync` method, the value and options parameters passed to the method can be modified within the `WriteThroughAsync` method.
``` csharp
protected override async Task<WriteThroughResult<WeatherForecastCacheValue>> WriteThroughAsync(
    WeatherForecastCacheValue value,
    CacheGrainEntryOptions options,
    CancellationToken ct)
{
  //process options/value as needed
  //log, write to db, etc...

  // return original or modified value/options to be used in the set operation, e.g.: write to db introduced sequence id to value
  // Value and Options returned here will be used to update the cache entry
  return new WriteThroughResult<WeatherForecastCacheValue>(Value: value, Options: options);
}
```

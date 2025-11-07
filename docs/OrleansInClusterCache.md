# Microsoft Orleans In-Cluster Cache

Abstract Microsoft Orleans grain implementations to cache data in memory (volatile) or can also save as grain state (persistent).

## ✨ Features

- Utilizes Orleans' built in features like request scheduling for cache stampede protection and serialization for passing cache data around,
- Simplifies architecture of a Microsoft Orleans project that needs a caching layer by providing it within Orleans itself, eliminating the need for a seperate caching server,
- Combines cache value creating factory method in the cache grain implementation, collecting all related business logic into a single unit,

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

Caching a value within in-cluster cache grains require inheriting one of the abstract base cache grains and implementing necessary GenerateValueAsync method. Type of the base cache grains are:

- `VolatileCacheGrain<TValue>` or `VolatileCacheGrain<TValue, TCreateArgs>` for storing data in memory,
- `PersistentCacheGrain<TValue>` or `PersistentCacheGrain<TValue, TCreateArgs>` for also persisting it as grain state,

> **Note**: Creating a marker interface inheriting `ICacheGrain<TValue>` or `ICacheGrain<TValue, TCreateArgs>` is helpful to organize and call grains.

> **Note**: Persistent cache requires a configured grain storage on Microsoft Orleans server.

Sample below inherits `VolatileCacheGrain<TValue, TCreateArgs>` and uses marker interface `IWeatherForecastCacheGrain`:
``` csharp
//marker interface
internal interface IWeatherForecastCacheGrain : ICacheGrain<WeatherForecastCacheValue, WeatherForecastCacheArgs>;

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

  protected override async Task<WeatherForecastCacheValue> GenerateValueAsync(
    WeatherForecastCacheArgs? args,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    var dayCount = args?.DayCount ?? 5;
    // Simulate a long-running operation
    await Task.Delay(5000, ct);
    return new WeatherForecastCacheValue()
    {
      Items = Enumerable.Range(1, dayCount).Select(index => new WeatherForecastCacheValueItem()
      {
        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = _summaries[Random.Shared.Next(_summaries.Length)]
      }).ToArray()
    };
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

Creating same cache grain with persistence requires implementation of `PersistentCacheGrain<TValue, TCreateArgs>` instead:
``` csharp
//marker interface (same as above)
internal interface IWeatherForecastCacheGrain : ICacheGrain<WeatherForecastCacheValue, WeatherForecastCacheArgs>;

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

  protected override async Task<WeatherForecastCacheValue> GenerateValueAsync(
    WeatherForecastCacheArgs? args,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    //Create cache value
    //Invoked when GetOrCreate or Create methods are called
    ///...
  }
}
```

## 🧩 How To Use

In-Cluster cache grains expose a couple of methods to interact with:

- `GetOrCreateAsync` method — fetch a cached value or create one via GenerateValue method if cached value is not found/has expired,
- `CreateAsync` method — create one via GenerateValue method,
- `SetAsync` method — store a value in cache,
- `TryGetAsync` method — fetch unexpired cached value if exists (updating last accessed time used for sliding expiration),
- `TryPeekAsync` method — fetch unexpired cached value if exists (without updating last accessed time used for sliding expiration),
- `RefreshAsync` method — update last accessed time used for sliding expiration if an unexpired cache value exists,
- `RemoveAsync` method — clear cache value

So calling the `WeatherForecastCacheGrain` sample implemented above to get cache value would be:

``` csharp
var args = new WeatherForecastCacheArgs(7);

await grainFactory.GetGrain<IWeatherForecastCacheGrain>("weatherforecast").GetOrCreateAsync(args, ct);
```

...where grainFactory is a Microsoft.Orleans `IGrainFactory` instance.

### Overriding options during generate value operation

Value generated by `GenerateValueAsync` method may contain data to modify caching options for that specific operation (e.g., it fetches token data including lifetime, and cache options should be modified according to that lifetime.). In that case, `GenerateValueAndOptionsAsync` method can be overridden, which is a wrapper method around `GenerateValueAsync`. This method returns both the generated value and the options to use when storing it in cache.

``` csharp
protected override async Task<(WeatherForecastCacheValue, CacheGrainEntryOptions)> GenerateValueAndOptionsAsync(
    WeatherForecastCacheArgs? args,
    CacheGrainEntryOptions options,
    CancellationToken ct)
{
    //Call base implementation to generate value
    (var value, options) = await base.GenerateValueAndOptionsAsync(args, options, ct);

    //process/override options/value as needed
    //...

    return (value, options);
}
```

### Processing value and options during set value operation
When setting a cache value directly via `SetAsync` method, the value and options can be processed/modified by overriding `ProcessValueAndOptionsAsync` method.
``` csharp
protected override async Task<(WeatherForecastCacheValue, CacheGrainEntryOptions)> ProcessValueAndOptionsAsync(
    WeatherForecastCacheValue value,
    CacheGrainEntryOptions options,
    CancellationToken ct)
{
  //process/override options/value as needed
  //log, write to db, etc...

  return (value, options);
}
```

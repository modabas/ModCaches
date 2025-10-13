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

> **Note**: `Action<InClusterCacheEntryOptions>? setupAction` parameter is optional and is used to configure default InClusterCacheEntryOptions value.

## 🧩 Implementation

Caching a value within InCluster cache grains require inheriting one of the abstract base cache grains and implementing necessary GenerateValueAsync method. Type of the base cache grains are:

- `VolatileInClusterCacheGrain<TValue>` or `VolatileInClusterCacheGrain<TValue, TCreateArgs>` for storing data in memory,
- `PersistentInClusterCacheGrain<TValue>` or `PersistentInClusterCacheGrain<TValue, TCreateArgs>` for also persisting it as grain state,

> **Note**: Creating a marker interface inheriting `IInClusterCacheGrain<TValue>` or `IInClusterCacheGrain<TValue, TCreateArgs>` is helpful to organize and call grains.

> **Note**: Persistent cache requires a configured grain storage on Microsoft Orleans server.

Sample below inherits `VolatileInClusterCacheGrain<TValue, TCreateArgs>` and uses marker interface `IWeatherForecastCacheGrain`:
``` csharp
//marker interface
internal interface IWeatherForecastCacheGrain : IInClusterCacheGrain<WeatherForecastCacheItem[], WeatherForecastCacheArgs>;

//grain implementation
internal class WeatherForecastCacheGrain :
  VolatileInClusterCacheGrain<WeatherForecastCacheItem[], WeatherForecastCacheArgs>,
  IWeatherForecastCacheGrain
{
  private static readonly string[] _summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

  public WeatherForecastCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  protected override async Task<WeatherForecastCacheItem[]> GenerateValueAsync(
    WeatherForecastCacheArgs? args,
    InClusterCacheEntryOptions options,
    CancellationToken ct)
  {
    //Create cache value
    //Invoked when GetOrCreate or Create methods are called

    var dayCount = args?.DayCount ?? 5;
    // Simulate a long-running operation
    await Task.Delay(5000, ct);
    return (Enumerable.Range(1, dayCount).Select(index =>
      new WeatherForecastCacheItem
      {
        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = _summaries[Random.Shared.Next(_summaries.Length)]
      }).ToArray());
  }
}

[GenerateSerializer]
internal struct WeatherForecastCacheItem
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

Creating same cache grain with persistence requires implementation of `PersistentInClusterCacheGrain<TValue, TCreateArgs>` instead:
``` csharp
//marker interface (same as above)
internal interface IWeatherForecastCacheGrain : IInClusterCacheGrain<WeatherForecastCacheItem[], WeatherForecastCacheArgs>;

//grain implementation
internal class WeatherForecastCacheGrain :
  PersistentInClusterCacheGrain<WeatherForecastCacheItem[], WeatherForecastCacheArgs>,
  IWeatherForecastCacheGrain
{
  private static readonly string[] _summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

  public WeatherForecastCacheGrain(
    IServiceProvider serviceProvider, 
    [PersistentState(nameof(WeatherForecastCacheGrain))]IPersistentState<InClusterCacheState<WeatherForecastCacheItem[]>> persistentState)
    : base(serviceProvider, persistentState)
  {
  }

  protected override async Task<WeatherForecastCacheItem[]> GenerateValueAsync(
    WeatherForecastCacheArgs? args,
    InClusterCacheEntryOptions options,
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

- GetOrCreate method to fetch a cached value or create one via GenerateValue method if cached value is not found/has expired,
- Create method to create one via GenerateValue method,
- Set method to store a value in cache,
- TryGet method to fetch unexpired cached value if exists (updating last accessed time used for sliding expiration),
- TryPeek method to fetch unexpired cached value if exists (without updating last accessed time used for sliding expiration),
- Refresh method to update last accessed time used for sliding expiration if an unexpired cache value exists,
- Remove method to clear cache value

So calling the `WeatherForecastCacheGrain` sample implemented above would be:

``` csharp
var args = new WeatherForecastCacheArgs(7);

await grainFactory.GetGrain<IWeatherForecastCacheGrain>("weatherforecast").GetOrCreateAsync(args, ct);
```

...where grainFactory is a Microsoft.Orleans `IGrainFactory` instance.
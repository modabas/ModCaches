# Microsoft Orleans Cluster Cache

Abstracts Microsoft Orleans grain implementations to cache data in memory (volatile) or persist it as grain state (persistent).

## ✨ Features

- Utilizes Orleans’ built-in features such as request scheduling for cache stampede protection and serialization for passing cache data around,  
- Simplifies the architecture of a Microsoft Orleans project that requires a caching layer by providing it within Orleans itself, eliminating the need for a separate caching server,  
- Supports Cache-Aside, Read-Through, Write-Around, and Write-Through caching strategies.

## 🛠️ Getting Started

### Install the NuGet Package

```bash
dotnet add package ModCaches.Orleans.Server
```

### Register Services

In your `Program.cs` of the Orleans Server:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOrleansClusterCache(options =>
{
  options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
  options.SlidingExpiration = TimeSpan.FromMinutes(1);
});
```

> **Note:** The `Action<ClusterCacheOptions>? setupAction` parameter is optional and can be used to configure default options.

## 🧩 Implementation

Implementing cluster cache grains is straightforward. However, it’s important to choose the appropriate caching strategy based on your application’s requirements. The available strategies are:

- **Cache-Aside:** The application code is responsible for checking the cache before fetching data from the source. If the data is not found in the cache, it is fetched from the source and then stored in the cache.  
- **Read-Through:** The cache itself retrieves data from the source when a cache miss occurs. The application only interacts with the cache, which handles data retrieval and storage.  
- **Write-Around:** The application writes data directly to the source, bypassing the cache. The cache is only updated when the data is read.  
- **Write-Through:** The application writes data to both the cache and the source simultaneously, ensuring the cache remains up to date.

To cache a value using cluster cache grains, start by creating a marker interface that inherits one or more of the following interfaces, depending on the caching strategy you want to use:

- `ICacheGrain<TValue>` for Cache-Aside and Write-Around strategies,  
- `IReadThroughCacheGrain<TValue>` or `IReadThroughCacheGrain<TValue, TStoreArgs>` for Read-Through strategies (inherits `ICacheGrain<TValue>`),  
- `IWriteThroughCacheGrain<TValue>` for Write-Through strategies (inherits `ICacheGrain<TValue>`).

Then create a cache grain implementation inheriting from one of the abstract base cache grain types and your marker interface:

- `VolatileCacheGrain<TValue>` or `VolatileCacheGrain<TValue, TStoreArgs>` for storing data in memory,  
- `PersistentCacheGrain<TValue>` or `PersistentCacheGrain<TValue, TStoreArgs>` for persisting data as grain state.

> **Note:** Read-Through cache grains require the implementation of `ReadThroughAsync` method to create cache entries when they are missing or expired.  
> Write-Through cache grains require the implementation of `WriteThroughAsync` method to handle write operations.  
> The default implementations of these methods throw `NotImplementedException`.

The following example implements the Read-Through cache pattern by creating a marker interface `IWeatherForecastCacheGrain` first and then creating an implementation of abstract `VolatileCacheGrain<TValue, TStoreArgs>` class, overriding the `ReadThroughAsync` method and inheriting marker interface:

```csharp
// marker interface
internal interface IWeatherForecastCacheGrain : IReadThroughCacheGrain<WeatherForecastCacheValue, WeatherForecastCacheArgs>;

// grain implementation
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

To utilize the Write-Through cache pattern, add the `IWriteThroughCacheGrain<TValue>` interface to the marker interface and override the `WriteThroughAsync` method in the cache grain class:

```csharp
// marker interface
internal interface IWeatherForecastCacheGrain : 
    IReadThroughCacheGrain<WeatherForecastCacheValue, WeatherForecastCacheArgs>,
    IWriteThroughCacheGrain<WeatherForecastCacheValue>;

// grain implementation
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

Creating a similar cache grain with state persistence requires implementing `PersistentCacheGrain<TValue, TStoreArgs>` instead:

> **Note:** Persistent cache requires a configured grain storage on the Microsoft Orleans server.

```csharp
// marker interface (same as original)
internal interface IWeatherForecastCacheGrain : IReadThroughCacheGrain<WeatherForecastCacheValue, WeatherForecastCacheArgs>;

// grain implementation
internal class WeatherForecastCacheGrain :
  PersistentCacheGrain<WeatherForecastCacheValue, WeatherForecastCacheArgs>,
  IWeatherForecastCacheGrain
{
  private static readonly string[] _summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

  public WeatherForecastCacheGrain(
    IServiceProvider serviceProvider, 
    [PersistentState(nameof(WeatherForecastCacheGrain))] IPersistentState<CacheState<WeatherForecastCacheValue>> persistentState)
    : base(serviceProvider, persistentState)
  {
  }

  protected override async Task<ReadThroughResult<WeatherForecastCacheValue>> ReadThroughAsync(
    WeatherForecastCacheArgs? args,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    // Create cache entry
    // Invoked when GetOrCreate or Create methods are called
    // ...
  }
}
```

## 🧩 How to Call Implemented Cache Grains

Cluster cache grains expose several methods for interaction:

- **`ICacheGrain<TValue>` methods:**
  1. `SetAsync` — stores a value in the cache and returns the stored value,  
  2. `TryGetAsync` — fetches an unexpired cached value if it exists (updating the last accessed time for sliding expiration),  
  3. `TryPeekAsync` — fetches an unexpired cached value if it exists (without updating the last accessed time),  
  4. `RefreshAsync` — updates the last accessed time if an unexpired cache value exists,  
  5. `RemoveAsync` — clears the cache value.  

- **`IReadThroughCacheGrain<TValue>` and `IReadThroughCacheGrain<TValue, TStoreArgs>` methods:**
  1. `GetOrCreateAsync` — fetches a cached value or creates one via `ReadThroughAsync` if the value is missing or expired,  
  2. `CreateAsync` — creates a cache value via `ReadThroughAsync` and fetches it,
  3. Inherits methods from `ICacheGrain<TValue>`.  

- **`IWriteThroughCacheGrain<TValue>` methods:**
  1. `SetAndWriteAsync` — writes the value via `WriteThroughAsync` and updates the cache,  
  2. Inherits methods from `ICacheGrain<TValue>`.

### Example usage of the Read-Through `WeatherForecastCacheGrain`:

- From within an Orleans silo, call the `GetOrCreateAsync` method to fetch or create a cached weather forecast:

```csharp
var args = new WeatherForecastCacheArgs(7);

var forecast = await grainFactory.GetGrain<IWeatherForecastCacheGrain>("weatherforecast").GetOrCreateAsync(args, ct);
```
>`grainFactory` is an instance of Microsoft Orleans `IGrainFactory`.


- From an Orleans client application, use the same approach to call the grain:
```csharp
var args = new WeatherForecastCacheArgs(7);

var forecast = await clusterClient.GetGrain<IWeatherForecastCacheGrain>("weatherforecast").GetOrCreateAsync(args, ct);
```
>`clusterClient` is an instance of Microsoft Orleans `IClusterClient`.

### Overriding Cache Options During Read-Through Operations

For a Read-Through cache, if you need to adjust caching options based on the generated value within the `ReadThroughAsync` method (for example, when the value includes a token with its own lifetime), return the updated options along with the value:

```csharp
protected override async Task<ReadThroughResult<WeatherForecastCacheValue>> ReadThroughAsync(
  WeatherForecastCacheArgs? args,
  CacheGrainEntryOptions options,
  CancellationToken ct)
{
  // Generate cache entry value, e.g., read from external service
  // var value = ...

  // Modify options as needed
  return new ReadThroughResult<WeatherForecastCacheValue>(
      Value: value, 
      Options: new CacheGrainEntryOptions(
          AbsoluteExpiration: default,
          AbsoluteExpirationRelativeToNow: TimeSpan.FromMinutes(5),
          SlidingExpiration: TimeSpan.FromMinutes(2)));
}
```

### Overriding Value and Cache Options During Write-Through Operations

For a Write-Through cache, it’s possible to modify the input value and options passed to the `SetAndWriteAsync` method. You can adjust these within `WriteThroughAsync` before returning them:

```csharp
protected override async Task<WriteThroughResult<WeatherForecastCacheValue>> WriteThroughAsync(
  WeatherForecastCacheValue value,
  CacheGrainEntryOptions options,
  CancellationToken ct)
{
  // Process options/value as needed (e.g., log, write to DB, etc.)

  // Return original or modified value/options to be used for the set operation
  return new WriteThroughResult<WeatherForecastCacheValue>(Value: value, Options: options);
}
```

using ModCaches.OrleansCaches.InCluster;
using ModEndpoints.Core;

namespace ShowcaseApi;

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

internal interface IWeatherForecastCacheGrain : IInClusterCacheGrain<WeatherForecastCacheItem[], WeatherForecastCacheArgs>
{
}

internal class WeatherForecastCacheGrain :
  InClusterCacheGrain<WeatherForecastCacheItem[], WeatherForecastCacheArgs>,
  IWeatherForecastCacheGrain
{
  private static readonly string[] _summaries =
  [
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching"
  ];

  public WeatherForecastCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  protected override async Task<WeatherForecastCacheItem[]> GenerateValueAsync(
    WeatherForecastCacheArgs? args,
    InClusterCacheEntryOptions options,
    CancellationToken ct)
  {
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

internal class GetWeatherForecast2(IGrainFactory grainFactory) : MinimalEndpoint<WeatherForecast[]>
{
  protected override void Configure(
      EndpointConfigurationBuilder builder,
      ConfigurationContext<EndpointConfigurationParameters> configurationContext)
  {
    builder.MapGet("/weatherforecast2")
      .WithName("GetWeatherForecast2")
      .WithTags("WeatherForecastWebApi");
  }

  protected override async Task<WeatherForecast[]> HandleAsync(CancellationToken ct)
  {
    var args = new WeatherForecastCacheArgs(7);

    return (await grainFactory
        .GetGrain<IWeatherForecastCacheGrain>("weatherforecast")
        .GetOrCreateAsync(args, ct))
      .Select(x => new WeatherForecast(
        Date: x.Date,
        TemperatureC: x.TemperatureC,
        Summary: x.Summary))
      .ToArray();
  }
}

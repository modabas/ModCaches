using ModCaches.ExtendedDistributedCache;
using ModEndpoints.Core;

namespace ShowcaseApi;

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal record WeatherForecastDistributedCacheItem(
  DateOnly Date,
  int TemperatureC,
  string? Summary);

internal class GetWeatherForecast(IExtendedDistributedCache cache) : MinimalEndpoint<WeatherForecast[]>
{
  private static readonly string[] _summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

  protected override void Configure(
      EndpointConfigurationBuilder builder,
      ConfigurationContext<EndpointConfigurationParameters> configurationContext)
  {
    builder.MapGet("/ExtendedDistributedCache")
      .WithName("ExtendedDistributedCache")
      .WithTags("CacheShowcaseWebApi");
  }

  protected override async Task<WeatherForecast[]> HandleAsync(CancellationToken ct)
  {
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

    return forecast
      .Select(w => new WeatherForecast(
        Date: w.Date,
        TemperatureC: w.TemperatureC,
        Summary: w.Summary))
      .ToArray();
  }
}

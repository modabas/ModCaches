using ModCaches.ExtendedDistributedCache;
using ModCaches.Orleans.Server.Cluster;
using ModCaches.Orleans.Server.Distributed;
using ModEndpoints.Core;
using ShowcaseApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCoHostedOrleansVolatileDistributedCache();

builder.Services.AddExtendedDistributedCache(options =>
{
  options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
});

builder.Services.AddOrleansClusterCache(options =>
{
  options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
  options.SlidingExpiration = TimeSpan.FromMinutes(1);
});

builder.Host.UseOrleans(siloBuilder =>
{
  siloBuilder.UseLocalhostClustering();
});

builder.Services.AddModEndpointsCoreFromAssemblyContaining<GetWeatherForecast>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapModEndpointsCore();

app.Run();



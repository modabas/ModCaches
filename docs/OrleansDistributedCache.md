# Microsoft Orleans Distributed Cache

Distributed cache implementations with Microsoft Orleans grains that keep data in memory (volatile) or can also save as grain state (persistent).

## ✨ Features

- Can be used from Orleans clients or from within Orleans servers,
- Implements standart IDistributedCache interface, making it a drop in replacement for any other distributed cache implementation utilizing same interface and can be used in conjunction with [Extended Distributed Cache](./ExtendedDistributedCache.md),
- Simplifies architecture of a Microsoft Orleans project that needs a caching layer by providing it within Orleans itself, eliminating the need for a seperate caching server

## 🛠️ Getting Started (Client-side)

### Install the NuGet Package:

```bash
dotnet add package ModCaches.Orleans.Client
```

### Register Services:

In your `Program.cs`:

``` csharp
var builder = WebApplication.CreateBuilder(args);

//for in memory caching
builder.Services.AddRemoteOrleansVolatileDistributedCache();

//... or for persistent caching
builder.Services.AddRemoteOrleansPersistentDistributedCache("persistentCache");
```

> **Note**: Both these methods have an optional `object? cacheDiKey` parameter that can be used to register an IDistributedCache as a keyed service.

> **Note**: Persistent cache requires configured default grain storage on Microsoft Orleans server.

## 🛠️ Getting Started (Server-side)

### Install the NuGet Package:

```bash
dotnet add package ModCaches.Orleans.Server
```

### Register Services:

In your `Program.cs`:

``` csharp
var builder = WebApplication.CreateBuilder(args);

//for in memory caching
builder.Services.AddCoHostedOrleansVolatileDistributedCache();

//... or for persistent caching
builder.Services.AddCoHostedOrleansPersistentDistributedCache("persistentCache");
```

> **Note**: Both these methods have an optional `object? cacheDiKey` parameter that can be used to register an IDistributedCache as a keyed service.

> **Note**: Persistent cache requires configured default grain storage on Microsoft Orleans server.

### Resolve from Dependency Injection

Resolve `IDistributedCache` via dependency injection (for example, constructor injection) to start using it.

``` csharp
public class MyService(IDistributedCache cache) 
{
}
```

or if `IDistributedCache` service in use is registered as a keyed service:

``` csharp
public class MyService([FromKeyedServices("persistentCache")]IDistributedCache cache) 
{
}
```
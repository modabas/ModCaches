using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Common;
using ModResults;

namespace ModCaches.Orleans.Server.Cluster;

/// <summary>
/// Intended to be used as an internal base class for cluster cache grain implementations like volatile and persistent cluster cache grains.
/// Implements basic in-memory caching logic along with Cache-Aside and Write-Around caching strategies.
/// Don't use directly, use derived classes instead.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public abstract class BaseCacheGrain<TValue>
  : BaseGrain,
  ICacheGrain<TValue>
  where TValue : notnull
{
  internal CacheEntry<TValue>? CacheEntry { get; set; }

  internal Func<DateTimeOffset> TimeProviderFunc { get; }

  internal CacheGrainEntryOptions DefaultEntryOptions { get; }

  internal bool HasSlidingExpiration =>
    CacheEntry?.HasSlidingExpiration ?? false;

  /// <summary>
  /// Marked as internal to prevent direct usage. Use derived classes instead.
  /// </summary>
  /// <param name="serviceProvider"></param>
  internal BaseCacheGrain(IServiceProvider serviceProvider)
  {
    var timeProvider = serviceProvider.GetService<TimeProvider>() ?? TimeProvider.System;
    TimeProviderFunc = () => timeProvider.GetUtcNow();
    var defaultOptions = serviceProvider.GetService<IOptions<ClusterCacheOptions>>() ??
      Options.Create(new ClusterCacheOptions());
    DefaultEntryOptions = defaultOptions.Value.ToCacheGrainEntryOptions();
  }

  public virtual Task RemoveAsync(CancellationToken ct)
  {
    RemoveInternal();
    return Task.CompletedTask;
  }

  public virtual Task<Result> RefreshAsync(CancellationToken ct)
  {
    if (CacheEntry is null ||
      !CacheEntry.TryGetValue(TimeProviderFunc, out _, out var expiresIn))
    {
      RemoveInternal();
      return Task.FromResult(Result.NotFound());
    }
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    DelayDeactivation(expiresIn.Value);
    return Task.FromResult(Result.Ok());
  }

  internal void RemoveInternal()
  {
    CacheEntry = null; // Remove the cache entry
    ResetDeactivation(); // Reset deactivation to default behavior
    return;
  }

  public virtual Task<Result<TValue>> GetAsync(CancellationToken ct)
  {
    if (CacheEntry?.TryGetValue(TimeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return Task.FromResult(Result<TValue>.Ok(value));
    }
    RemoveInternal();
    return Task.FromResult(Result<TValue>.NotFound());
  }

  public virtual Task<Result<TValue>> PeekAsync(CancellationToken ct)
  {
    if (CacheEntry?.TryPeekValue(TimeProviderFunc, out var value, out _) == true)
    {
      return Task.FromResult(Result<TValue>.Ok(value));
    }
    return Task.FromResult(Result<TValue>.NotFound());
  }

  public virtual Task SetAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var entryOptions = options ?? DefaultEntryOptions;
    SetInternal(value, entryOptions);
    return Task.CompletedTask;
  }

  internal void SetInternal(
    TValue value,
    CacheGrainEntryOptions options)
  {
    CacheEntry = new CacheEntry<TValue>(
      value,
      options.ToOrleansCacheEntryOptions(),
      TimeProviderFunc);
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    if (CacheEntry.TryGetExpiresIn(TimeProviderFunc, out var expiresIn))
    {
      DelayDeactivation(expiresIn.Value);
    }
    return;
  }
}

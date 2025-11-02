using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModCaches.Orleans.Server.Common;

namespace ModCaches.Orleans.Server.InCluster;

/// <summary>
/// Intended to be used as an internal base class for in-cluster cache grain implementations like volatile and persistent in-cluster cache grains. 
/// Don't use directly, use derived classes instead.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public abstract class BaseInClusterCacheGrain<TValue>
  : BaseGrain, IBaseInClusterCacheGrain<TValue>
  where TValue : notnull
{
  internal CacheEntry<TValue>? CacheEntry { get; set; }

  internal Func<DateTimeOffset> TimeProviderFunc { get; }

  internal IOptions<InClusterCacheEntryOptions> DefaultOptions { get; }

  internal bool HasSlidingExpiration =>
    CacheEntry?.HasSlidingExpiration ?? false;

  /// <summary>
  /// Marked as internal to prevent direct usage. Use derived classes instead.
  /// </summary>
  /// <param name="serviceProvider"></param>
  internal BaseInClusterCacheGrain(IServiceProvider serviceProvider)
  {
    var timeProvider = serviceProvider.GetService<TimeProvider>() ?? TimeProvider.System;
    TimeProviderFunc = () => timeProvider.GetUtcNow();
    DefaultOptions = serviceProvider.GetService<IOptions<InClusterCacheEntryOptions>>() ??
      Options.Create(new InClusterCacheEntryOptions());
  }

  public virtual Task RemoveAsync(CancellationToken ct)
  {
    RemoveInternal();
    return Task.CompletedTask;
  }

  public virtual Task<bool> RefreshAsync(CancellationToken ct)
  {
    if (CacheEntry is null ||
      !CacheEntry.TryGetValue(TimeProviderFunc, out _, out var expiresIn))
    {
      RemoveInternal();
      return Task.FromResult(false);
    }
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    DelayDeactivation(expiresIn.Value);
    return Task.FromResult(true);
  }

  private void RemoveInternal()
  {
    CacheEntry = null; // Remove the cache entry
    ResetDeactivation(); // Reset deactivation to default behavior
    return;
  }

  public virtual Task<(bool, TValue?)> TryGetAsync(CancellationToken ct)
  {
    if (CacheEntry?.TryGetValue(TimeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return Task.FromResult<(bool, TValue?)>((true, value));
    }
    RemoveInternal();
    return Task.FromResult<(bool, TValue?)>((false, default));
  }

  public virtual Task<(bool, TValue?)> TryPeekAsync(CancellationToken ct)
  {
    if (CacheEntry?.TryPeekValue(TimeProviderFunc, out var value, out _) == true)
    {
      return Task.FromResult<(bool, TValue?)>((true, value));
    }
    return Task.FromResult<(bool, TValue?)>((false, default));
  }

  public virtual Task SetAsync(
    TValue value,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var entryOptions = options ?? DefaultOptions.Value;
    CacheEntry = new CacheEntry<TValue>(
      value,
      entryOptions.ToOrleansCacheEntryOptions(),
      TimeProviderFunc);
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    if (CacheEntry.TryGetExpiresIn(TimeProviderFunc, out var expiresIn))
    {
      DelayDeactivation(expiresIn.Value);
    }
    return Task.CompletedTask;
  }
}

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

  internal CacheGrainEntryOptions DefaultEntryOptions { get; }

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
    var defaultOptions = serviceProvider.GetService<IOptions<InClusterCacheOptions>>() ??
      Options.Create(new InClusterCacheOptions());
    DefaultEntryOptions = defaultOptions.Value.ToCacheGrainEntryOptions();
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

  public virtual async Task SetAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var (entryValue, entryOptions) = await ProcessValueAndOptionsAsync(value, options ?? DefaultEntryOptions, ct);
    CacheEntry = new CacheEntry<TValue>(
      entryValue,
      entryOptions.ToOrleansCacheEntryOptions(),
      TimeProviderFunc);
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    if (CacheEntry.TryGetExpiresIn(TimeProviderFunc, out var expiresIn))
    {
      DelayDeactivation(expiresIn.Value);
    }
    return;
  }

  /// <summary>
  /// Used by SetAsync method before setting cache value. Can be used to process/override cache value and options.
  /// </summary>
  /// <param name="value">The value to set in the cache.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A tuple, data to be cached and cache options that will be used for the cache item.</returns>
  protected virtual Task<(TValue, CacheGrainEntryOptions)> ProcessValueAndOptionsAsync(TValue value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    return Task.FromResult((value, options));
  }
}

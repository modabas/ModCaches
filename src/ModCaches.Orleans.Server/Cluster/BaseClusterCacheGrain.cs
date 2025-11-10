using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Common;

namespace ModCaches.Orleans.Server.Cluster;

/// <summary>
/// Intended to be used as an internal base class for cluster cache grain implementations like volatile and persistent cluster cache grains. 
/// Don't use directly, use derived classes instead.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public abstract class BaseClusterCacheGrain<TValue>
  : BaseGrain,
  ICacheGrain<TValue>,
  IWriteThroughCacheGrain<TValue>
  where TValue : notnull
{
  internal CacheEntry<TValue>? CacheEntry { get; set; }

  internal Func<DateTimeOffset> TimeProviderFunc { get; }

  internal CacheGrainEntryOptions DefaultEntryOptions { get; }

  internal bool HasSlidingExpiration =>
    CacheEntry?.HasSlidingExpiration ?? false;

  private static readonly TryPeekResult<TValue> _emptyTryPeekResult =
    new(false, default);

  private static readonly TryGetResult<TValue> _emptyTryGetResult =
    new(false, default);

  /// <summary>
  /// Marked as internal to prevent direct usage. Use derived classes instead.
  /// </summary>
  /// <param name="serviceProvider"></param>
  internal BaseClusterCacheGrain(IServiceProvider serviceProvider)
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

  public virtual Task<TryGetResult<TValue>> TryGetAsync(CancellationToken ct)
  {
    if (CacheEntry?.TryGetValue(TimeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return Task.FromResult(new TryGetResult<TValue>(true, value));
    }
    RemoveInternal();
    return Task.FromResult(_emptyTryGetResult);
  }

  public virtual Task<TryPeekResult<TValue>> TryPeekAsync(CancellationToken ct)
  {
    if (CacheEntry?.TryPeekValue(TimeProviderFunc, out var value, out _) == true)
    {
      return Task.FromResult(new TryPeekResult<TValue>(true, value));
    }
    return Task.FromResult(_emptyTryPeekResult);
  }

  public virtual Task<TValue> SetAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var entryOptions = options ?? DefaultEntryOptions;
    CacheEntry = new CacheEntry<TValue>(
      value,
      entryOptions.ToOrleansCacheEntryOptions(),
      TimeProviderFunc);
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    if (CacheEntry.TryGetExpiresIn(TimeProviderFunc, out var expiresIn))
    {
      DelayDeactivation(expiresIn.Value);
    }
    return Task.FromResult(value);
  }

  public virtual async Task<TValue> SetAndWriteAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var entry = await WriteThroughAsync(value, options ?? DefaultEntryOptions, ct);
    CacheEntry = new CacheEntry<TValue>(
      entry.Value,
      entry.Options.ToOrleansCacheEntryOptions(),
      TimeProviderFunc);
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    if (CacheEntry.TryGetExpiresIn(TimeProviderFunc, out var expiresIn))
    {
      DelayDeactivation(expiresIn.Value);
    }
    return entry.Value;
  }

  /// <summary>
  /// Used by SetAndWriteAsync method before setting cache value. Intended to be used by write-through operation but also can be used to process/override cache value and options.
  /// </summary>
  /// <param name="value">The value to set in the cache.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A record containing value to be cached and cache options that will be used for the cache item.</returns>
  protected virtual Task<WriteThroughResult<TValue>> WriteThroughAsync(TValue value, CacheGrainEntryOptions options, CancellationToken ct)
  {
    throw new NotImplementedException("Override and implement WriteThroughAsync method in order to use write-through caching strategy.");
  }
}

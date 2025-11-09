using ModCaches.Orleans.Server.Common;

namespace ModCaches.Orleans.Server.InCluster;

/// <summary>
/// Intended to be used as an internal base class for in-cluster cache grain implementations like volatile and persistent in-cluster cache grains.
/// Don't use directly, use derived classes instead.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public abstract class BasicInClusterCacheGrain<TValue>
  : BaseInClusterCacheGrain<TValue>, IReadThroughCacheGrain<TValue>
  where TValue : notnull
{
  /// <summary>
  /// Marked as internal to prevent direct usage. Use derived classes instead.
  /// </summary>
  /// <param name="serviceProvider"></param>
  internal BasicInClusterCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  public virtual async Task<TValue> GetOrCreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var ret = await GetOrCreateInternalAsync(ct, options);
    return ret.Value;
  }

  internal async Task<(bool IsCreated, TValue Value)> GetOrCreateInternalAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    if (CacheEntry?.TryGetValue(TimeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return (IsCreated: false, Value: value);
    }
    return (IsCreated: true, Value: await CreateInternalAsync(ct, options));
  }

  public virtual Task<TValue> CreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return CreateInternalAsync(ct, options);
  }

  private async Task<TValue> CreateInternalAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var entry = await ReadThroughAsync(options ?? DefaultEntryOptions, ct);
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
  /// Value generation method for read-through caching strategy, used by GetOrCreateAsync and CreateAsync methods while creating a new cache value. Also can be used to override input cache options.
  /// </summary>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A record containing data to be cached and options to be used for caching.</returns>
  protected abstract Task<ReadThroughResult<TValue>> ReadThroughAsync(CacheGrainEntryOptions options, CancellationToken ct);
}

/// <summary>
/// Intended to be used as an internal base class for in-cluster cache grain implementations like volatile and persistent in-cluster cache grains.
/// Don't use directly, use derived classes instead.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
/// <typeparam name="TCreateArgs">Type of argument to be used during cache value generation.</typeparam>
public abstract class BasicInClusterCacheGrain<TValue, TCreateArgs>
  : BaseInClusterCacheGrain<TValue>, IReadThroughCacheGrain<TValue, TCreateArgs>
  where TValue : notnull
  where TCreateArgs : notnull
{
  /// <summary>
  /// Marked as internal to prevent direct usage. Use derived classes instead.
  /// </summary>
  /// <param name="serviceProvider"></param>
  internal BasicInClusterCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  public virtual async Task<TValue> GetOrCreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var (_, value) = await GetOrCreateInternalAsync(createArgs, ct, options);
    return value;
  }

  internal async Task<(bool IsCreated, TValue Value)> GetOrCreateInternalAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    if (CacheEntry?.TryGetValue(TimeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return (IsCreated: false, Value: value);
    }
    return (IsCreated: true, Value: await CreateInternalAsync(createArgs, ct, options));
  }

  public virtual Task<TValue> CreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return CreateInternalAsync(createArgs, ct, options);
  }

  private async Task<TValue> CreateInternalAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var entry = await ReadThroughAsync(createArgs, options ?? DefaultEntryOptions, ct);
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
  /// Value generation method for read-through caching strategy, used by GetOrCreateAsync and CreateAsync methods while creating a new cache value. Also can be used to override input cache options.
  /// </summary>
  /// <param name="args">Additional arguments to be used for value generation.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A record containing data to be cached and options to be used for caching.</returns>
  protected abstract Task<ReadThroughResult<TValue>> ReadThroughAsync(TCreateArgs? args, CacheGrainEntryOptions options, CancellationToken ct);
}

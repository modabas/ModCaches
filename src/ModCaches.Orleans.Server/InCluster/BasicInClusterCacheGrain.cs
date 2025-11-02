using ModCaches.Orleans.Server.Common;

namespace ModCaches.Orleans.Server.InCluster;

/// <summary>
/// Intended to be used as an internal base class for in-cluster cache grain implementations like volatile and persistent in-cluster cache grains.
/// Don't use directly, use derived classes instead.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public abstract class BasicInClusterCacheGrain<TValue>
  : BaseInClusterCacheGrain<TValue>, IInClusterCacheGrain<TValue>
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
    InClusterCacheEntryOptions? options = null)
  {
    var (_, value) = await GetOrCreateInternalAsync(ct, options);
    return value;
  }

  internal async Task<(bool, TValue)> GetOrCreateInternalAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    if (CacheEntry?.TryGetValue(TimeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return (false, value);
    }
    return (true, await CreateInternalAsync(ct, options));
  }

  public virtual Task<TValue> CreateAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    return CreateInternalAsync(ct, options);
  }

  private async Task<TValue> CreateInternalAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var entryOptions = options ?? DefaultOptions.Value;
    (var value, entryOptions) = await GenerateValueAndOptionsAsync(entryOptions, ct);
    CacheEntry = new CacheEntry<TValue>(
      value,
      entryOptions.ToOrleansCacheEntryOptions(),
      TimeProviderFunc);
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    if (CacheEntry.TryGetExpiresIn(TimeProviderFunc, out var expiresIn))
    {
      DelayDeactivation(expiresIn.Value);
    }
    return value;
  }

  /// <summary>
  /// Wrapper method over GenerateValueAsync method. Can be used to override cache options for the value.
  /// </summary>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A tuple, data to be cached and cache options that will be used for the cache item.</returns>
  protected virtual async Task<(TValue, InClusterCacheEntryOptions)> GenerateValueAndOptionsAsync(InClusterCacheEntryOptions options, CancellationToken ct)
  {
    var value = await GenerateValueAsync(options, ct);
    return (value, options);
  }

  /// <summary>
  /// Value generation method used by GetOrCreateAsync and CreateAsync methods while creating a new cache value.
  /// </summary>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>Data to be cached.</returns>
  protected abstract Task<TValue> GenerateValueAsync(InClusterCacheEntryOptions options, CancellationToken ct);
}

/// <summary>
/// Intended to be used as an internal base class for in-cluster cache grain implementations like volatile and persistent in-cluster cache grains.
/// Don't use directly, use derived classes instead.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
/// <typeparam name="TCreateArgs">Type of argument to be used during cache value generation.</typeparam>
public abstract class BasicInClusterCacheGrain<TValue, TCreateArgs>
  : BaseInClusterCacheGrain<TValue>, IInClusterCacheGrain<TValue, TCreateArgs>
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
    InClusterCacheEntryOptions? options = null)
  {
    var (_, value) = await GetOrCreateInternalAsync(createArgs, ct, options);
    return value;
  }

  internal async Task<(bool, TValue)> GetOrCreateInternalAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    if (CacheEntry?.TryGetValue(TimeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return (false, value);
    }
    return (true, await CreateInternalAsync(createArgs, ct, options));
  }

  public virtual Task<TValue> CreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    return CreateInternalAsync(createArgs, ct, options);
  }

  private async Task<TValue> CreateInternalAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var entryOptions = options ?? DefaultOptions.Value;
    (var value, entryOptions) = await GenerateValueAndOptionsAsync(createArgs, entryOptions, ct);
    CacheEntry = new CacheEntry<TValue>(
      value,
      entryOptions.ToOrleansCacheEntryOptions(),
      TimeProviderFunc);
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    if (CacheEntry.TryGetExpiresIn(TimeProviderFunc, out var expiresIn))
    {
      DelayDeactivation(expiresIn.Value);
    }
    return value;
  }

  /// <summary>
  /// Wrapper method over GenerateValueAsync method. Can be used to override cache options for the value.
  /// </summary>
  /// <param name="args">Additional arguments to be used for value generation.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A tuple, data to be cached and cache options that will be used for the cache item.</returns>
  protected virtual async Task<(TValue, InClusterCacheEntryOptions)> GenerateValueAndOptionsAsync(TCreateArgs? args, InClusterCacheEntryOptions options, CancellationToken ct)
  {
    var value = await GenerateValueAsync(args, options, ct);
    return (value, options);
  }

  /// <summary>
  /// Value generation method used by GetOrCreateAsync and CreateAsync methods while creating a new cache value.
  /// </summary>
  /// <param name="args">Additional arguments to be used for value generation.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>Data to be cached.</returns>
  protected abstract Task<TValue> GenerateValueAsync(TCreateArgs? args, InClusterCacheEntryOptions options, CancellationToken ct);
}

using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Common;

namespace ModCaches.Orleans.Server.Cluster;

/// <summary>
/// Intended to be used as an internal base class for cluster cache grain implementations like volatile and persistent cluster cache grains.
/// Don't use directly, use derived classes instead.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public abstract class BaseClusterCacheGrain<TValue>
  : BaseCacheGrain<TValue>,
  IReadThroughCacheGrain<TValue>,
  IWriteThroughCacheGrain<TValue>
  where TValue : notnull
{
  /// <summary>
  /// Marked as internal to prevent direct usage. Use derived classes instead.
  /// </summary>
  /// <param name="serviceProvider"></param>
  internal BaseClusterCacheGrain(IServiceProvider serviceProvider)
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
    var entry = await CreateFromStoreAsync(options ?? DefaultEntryOptions, ct);
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
  /// Value generation method for read-through caching strategy, used by GetOrCreateAsync and CreateAsync methods while creating a new cache value from underlying store. Also can be used to override input cache options.
  /// </summary>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A record containing data to be cached and options to be used for caching.</returns>
  protected virtual Task<CreateResult<TValue>> CreateFromStoreAsync(CacheGrainEntryOptions options, CancellationToken ct)
  {
    throw new NotImplementedException("Override and implement CreateFromStoreAsync method in order to use GetOrCreateAsync and CreateAsync methods.");
  }

  public virtual async Task<TValue> SetAndWriteAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var entry = await WriteToStoreAsync(value, options ?? DefaultEntryOptions, ct);
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
  protected virtual Task<WriteResult<TValue>> WriteToStoreAsync(
    TValue value,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    throw new NotImplementedException("Override and implement WriteToStoreAsync method in order to use SetAndWriteAsync method.");
  }

  public virtual async Task RemoveAndDeleteAsync(
    CancellationToken ct)
  {
    await DeleteFromStoreAsync(ct);
    RemoveInternal();
  }

  /// <summary>
  /// Used by RemoveAndDeleteAsync method before removing cache value. Intended to be used by write-through operation.
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns></returns>
  protected virtual Task DeleteFromStoreAsync(
    CancellationToken ct)
  {
    throw new NotImplementedException("Override and implement DeleteFromStoreAsync method in order to use RemoveAndDeleteAsync method.");
  }
}

/// <summary>
/// Intended to be used as an internal base class for cluster cache grain implementations like volatile and persistent cluster cache grains.
/// Don't use directly, use derived classes instead.
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
/// <typeparam name="TStoreArgs">Type of argument to be used during cache value generation.</typeparam>
public abstract class BaseClusterCacheGrain<TValue, TStoreArgs>
  : BaseCacheGrain<TValue>,
  IReadThroughCacheGrain<TValue, TStoreArgs>,
  IWriteThroughCacheGrain<TValue, TStoreArgs>
  where TValue : notnull
  where TStoreArgs : notnull
{
  /// <summary>
  /// Marked as internal to prevent direct usage. Use derived classes instead.
  /// </summary>
  /// <param name="serviceProvider"></param>
  internal BaseClusterCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  public virtual async Task<TValue> GetOrCreateAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var (_, value) = await GetOrCreateInternalAsync(args, ct, options);
    return value;
  }

  public Task<TValue> GetOrCreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return GetOrCreateAsync(default, ct, options);
  }

  internal async Task<(bool IsCreated, TValue Value)> GetOrCreateInternalAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    if (CacheEntry?.TryGetValue(TimeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return (IsCreated: false, Value: value);
    }
    return (IsCreated: true, Value: await CreateInternalAsync(args, ct, options));
  }

  public virtual Task<TValue> CreateAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return CreateInternalAsync(args, ct, options);
  }

  public Task<TValue> CreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return CreateAsync(default, ct, options);
  }

  private async Task<TValue> CreateInternalAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var entry = await CreateFromStoreAsync(args, options ?? DefaultEntryOptions, ct);
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
  /// <param name="args">Additional arguments to be used for value generation from underlying data store.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A record containing data to be cached and options to be used for caching.</returns>
  protected virtual Task<CreateResult<TValue>> CreateFromStoreAsync(
    TStoreArgs? args,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    throw new NotImplementedException("Override and implement CreateFromStoreAsync method in order to use GetOrCreateAsync and CreateAsync methods.");
  }

  public virtual async Task<TValue> SetAndWriteAsync(
    TStoreArgs? args,
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var entry = await WriteToStoreAsync(args, value, options ?? DefaultEntryOptions, ct);
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

  public Task<TValue> SetAndWriteAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return SetAndWriteAsync(default, value, ct, options);
  }

  /// <summary>
  /// Used by SetAndWriteAsync method before setting cache value. Intended to be used by write-through operation but also can be used to process/override cache value and options.
  /// </summary>
  /// <param name="args">Additional arguments to be used during write operation to underlying data store.</param>
  /// <param name="value">The value to set in the cache.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A record containing value to be cached and cache options that will be used for the cache item.</returns>
  protected virtual Task<WriteResult<TValue>> WriteToStoreAsync(
    TStoreArgs? args,
    TValue value,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    throw new NotImplementedException("Override and implement WriteToStoreAsync method in order to use SetAndWriteAsync method.");
  }

  public virtual async Task RemoveAndDeleteAsync(
    TStoreArgs? args,
    CancellationToken ct)
  {
    await DeleteFromStoreAsync(args, ct);
    RemoveInternal();
  }

  public Task RemoveAndDeleteAsync(
    CancellationToken ct)
  {
    return RemoveAndDeleteAsync(default, ct);
  }

  /// <summary>
  /// Used by RemoveAndDeleteAsync method before removing cache value. Intended to be used by write-through operation.
  /// </summary>
  /// <param name="args">Additional arguments to be used during delete operation from underlying data store.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns></returns>
  protected virtual Task DeleteFromStoreAsync(
    TStoreArgs? args,
    CancellationToken ct)
  {
    throw new NotImplementedException("Override and implement DeleteFromStoreAsync method in order to use RemoveAndDeleteAsync method.");
  }
}

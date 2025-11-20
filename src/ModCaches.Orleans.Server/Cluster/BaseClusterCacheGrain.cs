using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Common;
using ModResults;

namespace ModCaches.Orleans.Server.Cluster;

/// <summary>
/// Intended to be used as an internal base class for cluster cache grain implementations like volatile and persistent cluster cache grains.
/// Implements Read-Through and Write-Through caching strategies on top of BaseCacheGrain which implements basic in-memory caching logic along with Cache-Aside and Write-Around caching strategies.
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

  public virtual async Task<Result<TValue>> GetOrCreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var ret = await GetOrCreateInternalAsync(ct, options);
    return ret.ToResult(r => r.Value);
  }

  internal async Task<Result<(bool IsCreated, TValue Value)>> GetOrCreateInternalAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    if (CacheEntry?.TryGetValue(TimeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return (IsCreated: false, Value: value);
    }
    var result = await CreateInternalAsync(ct, options);
    return result.ToResult(r => (IsCreated: true, Value: r));
  }

  public virtual Task<Result<TValue>> CreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return CreateInternalAsync(ct, options);
  }

  private async Task<Result<TValue>> CreateInternalAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var result = await CreateFromStoreAsync(options ?? DefaultEntryOptions, ct);
    if (result.IsOk)
    {
      CacheEntry = new CacheEntry<TValue>(
        result.Value.Value,
        result.Value.Options.ToOrleansCacheEntryOptions(),
        TimeProviderFunc);
      // Delay deactivation to ensure it remains active while it has a valid cache entry
      if (CacheEntry.TryGetExpiresIn(TimeProviderFunc, out var expiresIn))
      {
        DelayDeactivation(expiresIn.Value);
      }
    }
    return result.ToResult(r => r.Value);
  }

  /// <summary>
  /// Reads data from backing data store. Used by GetOrCreateAsync and CreateAsync methods and cache value is set by the response from this method.<br/>
  /// Also can be used to override input cache options.<br/>
  /// Must be overridden in order to use any one of GetOrCreateAsync and CreateAsync methods.
  /// </summary>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A record containing data to be cached and options to be used for caching.</returns>
  protected virtual Task<Result<CreatedItem<TValue>>> CreateFromStoreAsync(CacheGrainEntryOptions options, CancellationToken ct)
  {
    throw new NotImplementedException("Override and implement CreateFromStoreAsync method in order to use GetOrCreateAsync and CreateAsync methods.");
  }

  public virtual async Task<Result<TValue>> SetAndWriteAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var result = await WriteToStoreAsync(value, options ?? DefaultEntryOptions, ct);
    if (result.IsOk)
    {
      SetInternal(result.Value.Value, result.Value.Options);
    }
    return result.ToResult(r => r.Value);
  }

  /// <summary>
  /// Performs update of the backing data store. Used by SetAndWriteAsync method and is called before setting cache value.<br/>
  /// Also can be used to process/override input cache value and options.<br/>
  /// Must be overridden in order to use SetAndWriteAsync method.
  /// </summary>
  /// <param name="value">The value to set in the cache.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A record containing value to be cached and cache options that will be used for the cache item.</returns>
  protected virtual Task<Result<WrittenItem<TValue>>> WriteToStoreAsync(
    TValue value,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    throw new NotImplementedException("Override and implement WriteToStoreAsync method in order to use SetAndWriteAsync method.");
  }

  public virtual async Task<Result> RemoveAndDeleteAsync(
    CancellationToken ct)
  {
    var result = await DeleteFromStoreAsync(ct);
    if (result.IsOk)
    {
      RemoveInternal();
    }
    return result;
  }

  /// <summary>
  /// Performs deletion the backing data store. Used by RemoveAndDeleteAsync method and is called before removing cache value.<br/>
  /// Must be overridden in order to use RemoveAndDeleteAsync method.
  /// </summary>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns></returns>
  protected virtual Task<Result> DeleteFromStoreAsync(
    CancellationToken ct)
  {
    throw new NotImplementedException("Override and implement DeleteFromStoreAsync method in order to use RemoveAndDeleteAsync method.");
  }
}

/// <summary>
/// Intended to be used as an internal base class for cluster cache grain implementations like volatile and persistent cluster cache grains.
/// Implements Read-Through and Write-Through caching strategies on top of BaseCacheGrain which implements basic in-memory caching logic along with Cache-Aside and Write-Around caching strategies.
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

  public virtual async Task<Result<TValue>> GetOrCreateAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var ret = await GetOrCreateInternalAsync(args, ct, options);
    return ret.ToResult(r => r.Value);
  }

  public Task<Result<TValue>> GetOrCreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return GetOrCreateAsync(default, ct, options);
  }

  internal async Task<Result<(bool IsCreated, TValue Value)>> GetOrCreateInternalAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    if (CacheEntry?.TryGetValue(TimeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return (IsCreated: false, Value: value);
    }
    var result = await CreateInternalAsync(args, ct, options);
    return result.ToResult(r => (IsCreated: true, Value: r));
  }

  public virtual Task<Result<TValue>> CreateAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return CreateInternalAsync(args, ct, options);
  }

  public Task<Result<TValue>> CreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return CreateAsync(default, ct, options);
  }

  private async Task<Result<TValue>> CreateInternalAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var result = await CreateFromStoreAsync(args, options ?? DefaultEntryOptions, ct);
    if (result.IsOk)
    {
      CacheEntry = new CacheEntry<TValue>(
        result.Value.Value,
        result.Value.Options.ToOrleansCacheEntryOptions(),
        TimeProviderFunc);
      // Delay deactivation to ensure it remains active while it has a valid cache entry
      if (CacheEntry.TryGetExpiresIn(TimeProviderFunc, out var expiresIn))
      {
        DelayDeactivation(expiresIn.Value);
      }
    }
    return result.ToResult(r => r.Value);
  }

  /// <summary>
  /// Reads data from backing data store. Used by GetOrCreateAsync and CreateAsync methods and cache value is set by the response from this method.<br/>
  /// Also can be used to override input cache options.<br/>
  /// Must be overridden in order to use any one of GetOrCreateAsync and CreateAsync methods.
  /// </summary>
  /// <param name="args">Parameters for underlying operations from backing data store.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A record containing data to be cached and options to be used for caching.</returns>
  protected virtual Task<Result<CreatedItem<TValue>>> CreateFromStoreAsync(
    TStoreArgs? args,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    throw new NotImplementedException("Override and implement CreateFromStoreAsync method in order to use GetOrCreateAsync and CreateAsync methods.");
  }

  public virtual async Task<Result<TValue>> SetAndWriteAsync(
    TStoreArgs? args,
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var result = await WriteToStoreAsync(args, value, options ?? DefaultEntryOptions, ct);
    if (result.IsOk)
    {
      SetInternal(result.Value.Value, result.Value.Options);
    }
    return result.ToResult(r => r.Value);
  }

  public Task<Result<TValue>> SetAndWriteAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return SetAndWriteAsync(default, value, ct, options);
  }

  /// <summary>
  /// Performs update of the backing data store. Used by SetAndWriteAsync method and is called before setting cache value.<br/>
  /// Also can be used to process/override input cache value and options.<br/>
  /// Must be overridden in order to use SetAndWriteAsync method.
  /// </summary>
  /// <param name="args">Parameters for underlying operations from backing data store.</param>
  /// <param name="value">The value to set in the cache.</param>
  /// <param name="options">The cache options for the value.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>A record containing value to be cached and cache options that will be used for the cache item.</returns>
  protected virtual Task<Result<WrittenItem<TValue>>> WriteToStoreAsync(
    TStoreArgs? args,
    TValue value,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    throw new NotImplementedException("Override and implement WriteToStoreAsync method in order to use SetAndWriteAsync method.");
  }

  public virtual async Task<Result> RemoveAndDeleteAsync(
    TStoreArgs? args,
    CancellationToken ct)
  {
    var result = await DeleteFromStoreAsync(args, ct);
    if (result.IsOk)
    {
      RemoveInternal();
    }
    return result;
  }

  public Task<Result> RemoveAndDeleteAsync(
    CancellationToken ct)
  {
    return RemoveAndDeleteAsync(default, ct);
  }

  /// <summary>
  /// Performs deletion the backing data store. Used by RemoveAndDeleteAsync method and is called before removing cache value.<br/>
  /// Must be overridden in order to use RemoveAndDeleteAsync method.
  /// </summary>
  /// <param name="args">Parameters for underlying operations from backing data store.</param>
  /// <param name="ct">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns></returns>
  protected virtual Task<Result> DeleteFromStoreAsync(
    TStoreArgs? args,
    CancellationToken ct)
  {
    throw new NotImplementedException("Override and implement DeleteFromStoreAsync method in order to use RemoveAndDeleteAsync method.");
  }
}

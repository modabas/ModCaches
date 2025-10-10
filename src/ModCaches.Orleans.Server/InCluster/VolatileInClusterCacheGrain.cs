using ModCaches.Orleans.Server.Common;

namespace ModCaches.Orleans.Server.InCluster;

public abstract class VolatileInClusterCacheGrain<TValue>
  : BaseInClusterCacheGrain<TValue>, IInClusterCacheGrain<TValue>
  where TValue : notnull
{
  public VolatileInClusterCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  public virtual async Task<TValue> GetOrCreateAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    if (CacheEntry?.TryGetValue(TimeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return value;
    }
    return await CreateInternalAsync(ct, options);
  }

  public virtual async Task<TValue> CreateAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    return await CreateInternalAsync(ct, options);
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

  protected virtual async Task<(TValue, InClusterCacheEntryOptions)> GenerateValueAndOptionsAsync(InClusterCacheEntryOptions options, CancellationToken ct)
  {
    var value = await GenerateValueAsync(options, ct);
    return (value, options);
  }

  protected abstract Task<TValue> GenerateValueAsync(InClusterCacheEntryOptions options, CancellationToken ct);
}

public abstract class VolatileInClusterCacheGrain<TValue, TCreateArgs>
  : BaseInClusterCacheGrain<TValue>, IInClusterCacheGrain<TValue, TCreateArgs>
  where TValue : notnull
  where TCreateArgs : notnull
{
  public VolatileInClusterCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  public virtual async Task<TValue> GetOrCreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    if (CacheEntry?.TryGetValue(TimeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return value;
    }
    return await CreateInternalAsync(createArgs, ct, options);
  }

  public virtual async Task<TValue> CreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    return await CreateInternalAsync(createArgs, ct, options);
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

  protected virtual async Task<(TValue, InClusterCacheEntryOptions)> GenerateValueAndOptionsAsync(TCreateArgs? args, InClusterCacheEntryOptions options, CancellationToken ct)
  {
    var value = await GenerateValueAsync(args, options, ct);
    return (value, options);
  }

  protected abstract Task<TValue> GenerateValueAsync(TCreateArgs? args, InClusterCacheEntryOptions options, CancellationToken ct);
}

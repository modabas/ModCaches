namespace ModCaches.Orleans.Server.InCluster;

/// <summary>
/// Abstract class to implement an in-cluster cache grain that keeps data in memory (volatile).
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public abstract class VolatileCacheGrain<TValue>
  : BaseCompositeCacheGrain<TValue>
  where TValue : notnull
{
  public VolatileCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  public sealed override Task<TValue> GetOrCreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return base.GetOrCreateAsync(ct, options);
  }

  public sealed override Task<TValue> CreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return base.CreateAsync(ct, options);
  }

  public sealed override Task<bool> RefreshAsync(CancellationToken ct)
  {
    return base.RefreshAsync(ct);
  }

  public sealed override Task RemoveAsync(CancellationToken ct)
  {
    return base.RemoveAsync(ct);
  }

  public sealed override Task<TValue> SetAsync(TValue value, CancellationToken ct, CacheGrainEntryOptions? options = null)
  {
    return base.SetAsync(value, ct, options);
  }

  public sealed override Task<TValue> SetAndWriteAsync(TValue value, CancellationToken ct, CacheGrainEntryOptions? options = null)
  {
    return base.SetAndWriteAsync(value, ct, options);
  }

  public sealed override Task<TryGetResult<TValue>> TryGetAsync(CancellationToken ct)
  {
    return base.TryGetAsync(ct);
  }

  public sealed override Task<TryPeekResult<TValue>> TryPeekAsync(CancellationToken ct)
  {
    return base.TryPeekAsync(ct);
  }
}

/// <summary>
/// Abstract class to implement an in-cluster cache grain that keeps data in memory (volatile).
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
/// <typeparam name="TCreateArgs">Type of argument to be used during cache value generation.</typeparam>
public abstract class VolatileCacheGrain<TValue, TCreateArgs>
  : BaseCompositeCacheGrain<TValue, TCreateArgs>
  where TValue : notnull
  where TCreateArgs : notnull
{
  public VolatileCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  public sealed override Task<TValue> GetOrCreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return base.GetOrCreateAsync(createArgs, ct, options);
  }

  public sealed override Task<TValue> CreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return base.CreateAsync(createArgs, ct, options);
  }

  public sealed override Task<bool> RefreshAsync(CancellationToken ct)
  {
    return base.RefreshAsync(ct);
  }

  public sealed override Task RemoveAsync(CancellationToken ct)
  {
    return base.RemoveAsync(ct);
  }

  public sealed override Task<TValue> SetAsync(TValue value, CancellationToken ct, CacheGrainEntryOptions? options = null)
  {
    return base.SetAsync(value, ct, options);
  }

  public sealed override Task<TValue> SetAndWriteAsync(TValue value, CancellationToken ct, CacheGrainEntryOptions? options = null)
  {
    return base.SetAndWriteAsync(value, ct, options);
  }

  public sealed override Task<TryGetResult<TValue>> TryGetAsync(CancellationToken ct)
  {
    return base.TryGetAsync(ct);
  }

  public sealed override Task<TryPeekResult<TValue>> TryPeekAsync(CancellationToken ct)
  {
    return base.TryPeekAsync(ct);
  }
}

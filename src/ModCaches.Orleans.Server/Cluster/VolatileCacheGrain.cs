using ModCaches.Orleans.Abstractions.Cluster;

namespace ModCaches.Orleans.Server.Cluster;

/// <summary>
/// Abstract class to implement a cluster cache grain that keeps data in memory (volatile).
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public abstract class VolatileCacheGrain<TValue>
  : BaseClusterCacheGrain<TValue>
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

  public sealed override Task RemoveAndDeleteAsync(CancellationToken ct)
  {
    return base.RemoveAndDeleteAsync(ct);
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
/// Abstract class to implement a cluster cache grain that keeps data in memory (volatile).
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
/// <typeparam name="TStoreArgs">Type of argument to be used during cache value generation.</typeparam>
public abstract class VolatileCacheGrain<TValue, TStoreArgs>
  : BaseClusterCacheGrain<TValue, TStoreArgs>
  where TValue : notnull
  where TStoreArgs : notnull
{
  public VolatileCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  public sealed override Task<TValue> GetOrCreateAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return base.GetOrCreateAsync(args, ct, options);
  }

  public sealed override Task<TValue> CreateAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return base.CreateAsync(args, ct, options);
  }

  public sealed override Task<bool> RefreshAsync(CancellationToken ct)
  {
    return base.RefreshAsync(ct);
  }

  public sealed override Task RemoveAsync(CancellationToken ct)
  {
    return base.RemoveAsync(ct);
  }

  public sealed override Task RemoveAndDeleteAsync(TStoreArgs? args, CancellationToken ct)
  {
    return base.RemoveAndDeleteAsync(args, ct);
  }

  public sealed override Task<TValue> SetAsync(TValue value, CancellationToken ct, CacheGrainEntryOptions? options = null)
  {
    return base.SetAsync(value, ct, options);
  }

  public sealed override Task<TValue> SetAndWriteAsync(
    TStoreArgs? args,
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    return base.SetAndWriteAsync(args, value, ct, options);
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

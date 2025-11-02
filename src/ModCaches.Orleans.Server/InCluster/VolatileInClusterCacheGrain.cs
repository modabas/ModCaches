using ModCaches.Orleans.Server.Common;

namespace ModCaches.Orleans.Server.InCluster;

/// <summary>
/// Abstract class to implement an in-cluster cache grain that keeps data in memory (volatile).
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public abstract class VolatileInClusterCacheGrain<TValue>
  : BasicInClusterCacheGrain<TValue>, IInClusterCacheGrain<TValue>
  where TValue : notnull
{
  public VolatileInClusterCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  public sealed override Task<TValue> GetOrCreateAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    return base.GetOrCreateAsync(ct, options);
  }

  public sealed override Task<TValue> CreateAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
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

  public sealed override Task SetAsync(TValue value, CancellationToken ct, InClusterCacheEntryOptions? options = null)
  {
    return base.SetAsync(value, ct, options);
  }

  public sealed override Task<(bool, TValue?)> TryGetAsync(CancellationToken ct)
  {
    return base.TryGetAsync(ct);
  }

  public sealed override Task<(bool, TValue?)> TryPeekAsync(CancellationToken ct)
  {
    return base.TryPeekAsync(ct);
  }
}

/// <summary>
/// Abstract class to implement an in-cluster cache grain that keeps data in memory (volatile).
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
/// <typeparam name="TCreateArgs">Type of argument to be used during cache value generation.</typeparam>
public abstract class VolatileInClusterCacheGrain<TValue, TCreateArgs>
  : BasicInClusterCacheGrain<TValue, TCreateArgs>, IInClusterCacheGrain<TValue, TCreateArgs>
  where TValue : notnull
  where TCreateArgs : notnull
{
  public VolatileInClusterCacheGrain(IServiceProvider serviceProvider)
    : base(serviceProvider)
  {
  }

  public sealed override Task<TValue> GetOrCreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    return base.GetOrCreateAsync(createArgs, ct, options);
  }

  public sealed override Task<TValue> CreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
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

  public sealed override Task SetAsync(TValue value, CancellationToken ct, InClusterCacheEntryOptions? options = null)
  {
    return base.SetAsync(value, ct, options);
  }

  public sealed override Task<(bool, TValue?)> TryGetAsync(CancellationToken ct)
  {
    return base.TryGetAsync(ct);
  }

  public sealed override Task<(bool, TValue?)> TryPeekAsync(CancellationToken ct)
  {
    return base.TryPeekAsync(ct);
  }
}

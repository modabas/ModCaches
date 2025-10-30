using ModCaches.Orleans.Server.Common;

namespace ModCaches.Orleans.Server.InCluster;

/// <summary>
/// Abstract class to implement an in-cluster cache grain that keeps data in memory and also saves it as grain state (persistent).
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public abstract class PersistentInClusterCacheGrain<TValue>
  : ExtensibleInClusterCacheGrain<TValue>, IInClusterCacheGrain<TValue>
  where TValue : notnull
{
  private bool _stateCleared = false;

  internal IPersistentState<InClusterCacheState<TValue>> PersistentState { get; set; }

  public PersistentInClusterCacheGrain(IServiceProvider serviceProvider,
    IPersistentState<InClusterCacheState<TValue>> persistentState)
    : base(serviceProvider)
  {
    PersistentState = persistentState;
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    await base.OnActivateAsync(cancellationToken);
    if (PersistentState.RecordExists &&
      PersistentState.State.Value is not null &&
      PersistentState.State.LastAccessed > DateTimeOffset.MinValue)
    {
      CacheEntry = new CacheEntry<TValue>(
        PersistentState.State.Value,
        PersistentState.State.AbsoluteExpiration,
        PersistentState.State.SlidingExpiration,
        PersistentState.State.LastAccessed);
    }
  }

  public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
  {
    if (!_stateCleared)
    {
      if (CacheEntry is null ||
        !CacheEntry.TryPeekValue(TimeProviderFunc, out _, out _))
      {
        await ClearStateAsync(cancellationToken);
      }
    }
    await base.OnDeactivateAsync(reason, cancellationToken);
  }

  public sealed override async Task<TValue> GetOrCreateAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var ret = await base.GetOrCreateAsync(ct, options);
    await WriteStateAsync(ct);
    return ret;
  }

  public sealed override async Task<TValue> CreateAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var ret = await base.CreateAsync(ct, options);
    await WriteStateAsync(ct);
    return ret;
  }

  public sealed override async Task<bool> RefreshAsync(CancellationToken ct)
  {
    var ret = await base.RefreshAsync(ct);
    if (ret)
    {
      await WriteStateAsync(ct);
    }
    else
    {
      await ClearStateAsync(ct);
    }
    return ret;
  }

  public sealed override async Task RemoveAsync(CancellationToken ct)
  {
    await base.RemoveAsync(ct);
    await ClearStateAsync(ct);
  }

  public sealed override async Task SetAsync(
    TValue value,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    await base.SetAsync(value, ct, options);
    await WriteStateAsync(ct);
  }

  public sealed override async Task<(bool, TValue?)> TryGetAsync(CancellationToken ct)
  {
    var ret = await base.TryGetAsync(ct);
    if (ret.Item1)
    {
      await WriteStateAsync(ct);
    }
    else
    {
      await ClearStateAsync(ct);
    }
    return ret;
  }
  public sealed override Task<(bool, TValue?)> TryPeekAsync(CancellationToken ct)
  {
    return base.TryPeekAsync(ct);
  }

  private async Task WriteStateAsync(CancellationToken ct)
  {
    //This is the expected case where we have a valid cache entry to write
    if (CacheEntry is not null)
    {
      PersistentState.State = CacheEntry.ToState();
      await PersistentState.WriteStateAsync(ct);
      _stateCleared = false;
    }
  }

  private async Task ClearStateAsync(CancellationToken ct)
  {
    if (PersistentState.RecordExists)
    {
      await PersistentState.ClearStateAsync(ct);
    }
    _stateCleared = true;
  }
}

/// <summary>
/// Abstract class to implement an in-cluster cache grain that keeps data in memory and also saves it as grain state (persistent).
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
/// <typeparam name="TCreateArgs">Type of argument to be used during cache value generation.</typeparam>
public abstract class PersistentInClusterCacheGrain<TValue, TCreateArgs>
  : ExtensibleInClusterCacheGrain<TValue, TCreateArgs>, IInClusterCacheGrain<TValue, TCreateArgs>
  where TValue : notnull
  where TCreateArgs : notnull
{
  private bool _stateCleared = false;
  internal IPersistentState<InClusterCacheState<TValue>> PersistentState { get; set; }
  public PersistentInClusterCacheGrain(IServiceProvider serviceProvider,
    IPersistentState<InClusterCacheState<TValue>> persistentState)
    : base(serviceProvider)
  {
    PersistentState = persistentState;
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    await base.OnActivateAsync(cancellationToken);
    if (PersistentState.RecordExists &&
      PersistentState.State.Value is not null &&
      PersistentState.State.LastAccessed > DateTimeOffset.MinValue)
    {
      CacheEntry = new CacheEntry<TValue>(
        PersistentState.State.Value,
        PersistentState.State.AbsoluteExpiration,
        PersistentState.State.SlidingExpiration,
        PersistentState.State.LastAccessed);
    }
  }

  public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
  {
    if (!_stateCleared)
    {
      if (CacheEntry is null ||
        !CacheEntry.TryPeekValue(TimeProviderFunc, out _, out _))
      {
        await ClearStateAsync(cancellationToken);
      }
    }
    await base.OnDeactivateAsync(reason, cancellationToken);
  }

  public sealed override async Task<TValue> GetOrCreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var ret = await base.GetOrCreateAsync(createArgs, ct, options);
    await WriteStateAsync(ct);
    return ret;
  }

  public sealed override async Task<TValue> CreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var ret = await base.CreateAsync(createArgs, ct, options);
    await WriteStateAsync(ct);
    return ret;
  }

  public sealed override async Task<bool> RefreshAsync(CancellationToken ct)
  {
    var ret = await base.RefreshAsync(ct);
    if (ret)
    {
      await WriteStateAsync(ct);
    }
    else
    {
      await ClearStateAsync(ct);
    }
    return ret;
  }

  public sealed override async Task RemoveAsync(CancellationToken ct)
  {
    await base.RemoveAsync(ct);
    await ClearStateAsync(ct);
  }

  public sealed override async Task SetAsync(
    TValue value,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    await base.SetAsync(value, ct, options);
    await WriteStateAsync(ct);
  }

  public sealed override async Task<(bool, TValue?)> TryGetAsync(CancellationToken ct)
  {
    var ret = await base.TryGetAsync(ct);
    if (ret.Item1)
    {
      await WriteStateAsync(ct);
    }
    else
    {
      await ClearStateAsync(ct);
    }
    return ret;
  }

  public sealed override Task<(bool, TValue?)> TryPeekAsync(CancellationToken ct)
  {
    return base.TryPeekAsync(ct);
  }

  private async Task WriteStateAsync(CancellationToken ct)
  {
    //This is the expected case where we have a valid cache entry to write
    if (CacheEntry is not null)
    {
      PersistentState.State = CacheEntry.ToState();
      await PersistentState.WriteStateAsync(ct);
      _stateCleared = false;
    }
  }

  private async Task ClearStateAsync(CancellationToken ct)
  {
    if (PersistentState.RecordExists)
    {
      await PersistentState.ClearStateAsync(ct);
    }
    _stateCleared = true;
  }
}

using ModCaches.Orleans.Server.Common;

namespace ModCaches.Orleans.Server.InCluster;

/// <summary>
/// Abstract class to implement an in-cluster cache grain that keeps data in memory and also saves it as grain state (persistent).
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public abstract class PersistentInClusterCacheGrain<TValue>
  : VolatileInClusterCacheGrain<TValue>, IInClusterCacheGrain<TValue>
  where TValue : notnull
{
  private bool _stateCleared = false;
  private readonly IPersistentState<InClusterCacheState<TValue>> _persistentState;

  public PersistentInClusterCacheGrain(IServiceProvider serviceProvider,
    IPersistentState<InClusterCacheState<TValue>> persistentState)
    : base(serviceProvider)
  {
    _persistentState = persistentState;
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    await base.OnActivateAsync(cancellationToken);
    if (_persistentState.RecordExists &&
      _persistentState.State.Value is not null &&
      _persistentState.State.LastAccessed > DateTimeOffset.MinValue)
    {
      CacheEntry = new CacheEntry<TValue>(
        _persistentState.State.Value,
        _persistentState.State.AbsoluteExpiration,
        _persistentState.State.SlidingExpiration,
        _persistentState.State.LastAccessed);

      await RefreshInternalAsync(cancellationToken);
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

  public override async Task<TValue> GetOrCreateAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var ret = await base.GetOrCreateAsync(ct, options);
    await WriteStateAsync(ct);
    return ret;
  }

  public override async Task<TValue> CreateAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var ret = await base.CreateAsync(ct, options);
    await WriteStateAsync(ct);
    return ret;
  }

  public override Task<bool> RefreshAsync(CancellationToken ct)
  {
    return RefreshInternalAsync(ct);
  }

  private async Task<bool> RefreshInternalAsync(CancellationToken ct)
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

  public override async Task RemoveAsync(CancellationToken ct)
  {
    await base.RemoveAsync(ct);
    await ClearStateAsync(ct);
  }

  public override async Task SetAsync(
    TValue value,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    await base.SetAsync(value, ct, options);
    await WriteStateAsync(ct);
  }

  public override async Task<(bool, TValue?)> TryGetAsync(CancellationToken ct)
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

  private async Task WriteStateAsync(CancellationToken ct)
  {
    //This is the expected case where we have a valid cache entry to write
    if (CacheEntry is not null)
    {
      _persistentState.State = CacheEntry.ToState();
      await _persistentState.WriteStateAsync(ct);
      _stateCleared = false;
    }
  }

  private async Task ClearStateAsync(CancellationToken ct)
  {
    if (!_stateCleared && _persistentState.RecordExists)
    {
      await _persistentState.ClearStateAsync(ct);
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
  : VolatileInClusterCacheGrain<TValue, TCreateArgs>, IInClusterCacheGrain<TValue, TCreateArgs>
  where TValue : notnull
  where TCreateArgs : notnull
{
  private bool _stateCleared = false;
  private readonly IPersistentState<InClusterCacheState<TValue>> _persistentState;

  public PersistentInClusterCacheGrain(IServiceProvider serviceProvider,
    IPersistentState<InClusterCacheState<TValue>> persistentState)
    : base(serviceProvider)
  {
    _persistentState = persistentState;
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    await base.OnActivateAsync(cancellationToken);
    if (_persistentState.RecordExists &&
      _persistentState.State.Value is not null &&
      _persistentState.State.LastAccessed > DateTimeOffset.MinValue)
    {
      CacheEntry = new CacheEntry<TValue>(
        _persistentState.State.Value,
        _persistentState.State.AbsoluteExpiration,
        _persistentState.State.SlidingExpiration,
        _persistentState.State.LastAccessed);

      await RefreshInternalAsync(cancellationToken);
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

  public override async Task<TValue> GetOrCreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var ret = await base.GetOrCreateAsync(createArgs, ct, options);
    await WriteStateAsync(ct);
    return ret;
  }

  public override async Task<TValue> CreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var ret = await base.CreateAsync(createArgs, ct, options);
    await WriteStateAsync(ct);
    return ret;
  }

  public override Task<bool> RefreshAsync(CancellationToken ct)
  {
    return RefreshInternalAsync(ct);
  }

  private async Task<bool> RefreshInternalAsync(CancellationToken ct)
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

  public override async Task RemoveAsync(CancellationToken ct)
  {
    await base.RemoveAsync(ct);
    await ClearStateAsync(ct);
  }

  public override async Task SetAsync(
    TValue value,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    await base.SetAsync(value, ct, options);
    await WriteStateAsync(ct);
  }

  public override async Task<(bool, TValue?)> TryGetAsync(CancellationToken ct)
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

  private async Task WriteStateAsync(CancellationToken ct)
  {
    //This is the expected case where we have a valid cache entry to write
    if (CacheEntry is not null)
    {
      _persistentState.State = CacheEntry.ToState();
      await _persistentState.WriteStateAsync(ct);
      _stateCleared = false;
    }
  }

  private async Task ClearStateAsync(CancellationToken ct)
  {
    if (!_stateCleared && _persistentState.RecordExists)
    {
      await _persistentState.ClearStateAsync(ct);
    }
    _stateCleared = true;
  }
}

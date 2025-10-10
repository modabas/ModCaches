using Microsoft.Extensions.Caching.Memory;
using ModCaches.OrleansCaches.Common;
using Orleans;
using Orleans.Runtime;

namespace ModCaches.OrleansCaches.InCluster;

public abstract class PersistentInClusterCacheGrain<TValue>
  : VolatileInClusterCacheGrain<TValue>, IInClusterCacheGrain<TValue>
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
        !CacheEntry.TryGetValue(TimeProviderFunc, out _, out _))
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

  public override async Task<bool> RefreshAsync(CancellationToken ct)
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
    PersistentState.State = CacheEntry!.ToState();
    await PersistentState.WriteStateAsync(ct);
    _stateCleared = false;
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

public abstract class PersistentInClusterCacheGrain<TValue, TCreateArgs>
  : VolatileInClusterCacheGrain<TValue, TCreateArgs>, IInClusterCacheGrain<TValue, TCreateArgs>
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
        !CacheEntry.TryGetValue(TimeProviderFunc, out _, out _))
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

  public override async Task<bool> RefreshAsync(CancellationToken ct)
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
    PersistentState.State = CacheEntry!.ToState();
    await PersistentState.WriteStateAsync(ct);
    _stateCleared = false;
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

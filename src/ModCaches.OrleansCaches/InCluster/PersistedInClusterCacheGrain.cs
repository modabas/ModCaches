using ModCaches.OrleansCaches.Common;
using Orleans.Runtime;

namespace ModCaches.OrleansCaches.InCluster;

public abstract class PersistedInClusterCacheGrain<TValue>
  : InClusterCacheGrain<TValue>, IInClusterCacheGrain<TValue>
  where TValue : notnull
{
  internal IPersistentState<InClusterCacheState<TValue>> PersistentState { get; set; }
  public PersistedInClusterCacheGrain(IServiceProvider serviceProvider,
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

  public override async Task<TValue> GetOrCreateAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var cached = await base.GetOrCreateAsync(ct, options);
    await WriteStateAsync(ct);
    return cached;
  }

  public override async Task<TValue> CreateAsync(
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var cached = await base.CreateAsync(ct, options);
    await WriteStateAsync(ct);
    return cached;
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
    await PersistentState.ClearStateAsync(ct);
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
  }

  private async Task ClearStateAsync(CancellationToken ct)
  {
    if (PersistentState.RecordExists)
    {
      await PersistentState.ClearStateAsync(ct);
    }
  }
}

public abstract class PersistedInClusterCacheGrain<TValue, TCreateArgs>
  : InClusterCacheGrain<TValue, TCreateArgs>, IInClusterCacheGrain<TValue, TCreateArgs>
  where TValue : notnull
  where TCreateArgs : notnull
{
  internal IPersistentState<InClusterCacheState<TValue>> PersistentState { get; set; }
  public PersistedInClusterCacheGrain(IServiceProvider serviceProvider,
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

  public override async Task<TValue> GetOrCreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var cached = await base.GetOrCreateAsync(createArgs, ct, options);
    await WriteStateAsync(ct);
    return cached;
  }

  public override async Task<TValue> CreateAsync(
    TCreateArgs? createArgs,
    CancellationToken ct,
    InClusterCacheEntryOptions? options = null)
  {
    var cached = await base.CreateAsync(createArgs, ct, options);
    await WriteStateAsync(ct);
    return cached;
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
    await PersistentState.ClearStateAsync(ct);
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
  }

  private async Task ClearStateAsync(CancellationToken ct)
  {
    if (PersistentState.RecordExists)
    {
      await PersistentState.ClearStateAsync(ct);
    }
  }
}

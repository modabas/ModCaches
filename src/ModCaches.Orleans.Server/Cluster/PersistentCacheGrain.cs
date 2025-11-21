using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Common;
using ModResults;

namespace ModCaches.Orleans.Server.Cluster;

/// <summary>
/// Abstract class to implement a cluster cache grain that keeps data in memory and also saves it as grain state (persistent).
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
public abstract class PersistentCacheGrain<TValue>
  : BaseClusterCacheGrain<TValue>
  where TValue : notnull
{
  private bool _stateCleared = false;
  private readonly IPersistentState<CacheState<TValue>> _persistentState;

  public PersistentCacheGrain(IServiceProvider serviceProvider,
    IPersistentState<CacheState<TValue>> persistentState)
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

  public sealed override async Task<Result<TValue>> GetOrCreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var ret = await GetOrCreateInternalAsync(ct, options);
    if (ret.IsOk)
    {
      if (ret.Value.IsCreated || HasSlidingExpiration)
      {
        await WriteStateAsync(ct);
      }
    }
    return ret.ToResult(r => r.Value);
  }

  public sealed override async Task<Result<TValue>> CreateAsync(
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var ret = await base.CreateAsync(ct, options);
    if (ret.IsOk)
    {
      await WriteStateAsync(ct);
    }
    return ret;
  }

  public sealed override async Task<Result> RefreshAsync(CancellationToken ct)
  {
    var ret = await base.RefreshAsync(ct);
    if (ret.IsOk)
    {
      // Only write state if we have sliding expiration, as absolute expiration does not change on access
      if (HasSlidingExpiration)
      {
        await WriteStateAsync(ct);
      }
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

  public sealed override async Task<Result> RemoveAndDeleteAsync(CancellationToken ct)
  {
    var ret = await base.RemoveAndDeleteAsync(ct);
    if (ret.IsOk)
    {
      await ClearStateAsync(ct);
    }
    return ret;
  }

  public sealed override async Task SetAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    await base.SetAsync(value, ct, options);
    await WriteStateAsync(ct);
  }

  public sealed override async Task<Result<TValue>> SetAndWriteAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var ret = await base.SetAndWriteAsync(value, ct, options);
    if (ret.IsOk)
    {
      await WriteStateAsync(ct);
    }
    return ret;
  }

  public sealed override async Task<Result<TValue>> GetAsync(CancellationToken ct)
  {
    var ret = await base.GetAsync(ct);
    if (ret.IsOk)
    {
      // Only write state if we have sliding expiration, as absolute expiration does not change on access
      if (HasSlidingExpiration)
      {
        await WriteStateAsync(ct);
      }
    }
    else
    {
      await ClearStateAsync(ct);
    }
    return ret;
  }

  public sealed override Task<Result<TValue>> PeekAsync(CancellationToken ct)
  {
    return base.PeekAsync(ct);
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
/// Abstract class to implement a cluster cache grain that keeps data in memory and also saves it as grain state (persistent).
/// </summary>
/// <typeparam name="TValue">Type of the cache data.</typeparam>
/// <typeparam name="TStoreArgs">Type of argument to be used during cache value generation.</typeparam>
public abstract class PersistentCacheGrain<TValue, TStoreArgs>
  : BaseClusterCacheGrain<TValue, TStoreArgs>
  where TValue : notnull
  where TStoreArgs : notnull
{
  private bool _stateCleared = false;
  private readonly IPersistentState<CacheState<TValue>> _persistentState;

  public PersistentCacheGrain(IServiceProvider serviceProvider,
    IPersistentState<CacheState<TValue>> persistentState)
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

  public sealed override async Task<Result<TValue>> GetOrCreateAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var ret = await GetOrCreateInternalAsync(args, ct, options);
    if (ret.IsOk)
    {
      if (ret.Value.IsCreated || HasSlidingExpiration)
      {
        await WriteStateAsync(ct);
      }
    }
    return ret.ToResult(r => r.Value);
  }

  public sealed override async Task<Result<TValue>> CreateAsync(
    TStoreArgs? args,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var ret = await base.CreateAsync(args, ct, options);
    if (ret.IsOk)
    {
      await WriteStateAsync(ct);
    }
    return ret;
  }

  public sealed override async Task<Result> RefreshAsync(CancellationToken ct)
  {
    var ret = await base.RefreshAsync(ct);
    if (ret.IsOk)
    {
      // Only write state if we have sliding expiration, as absolute expiration does not change on access
      if (HasSlidingExpiration)
      {
        await WriteStateAsync(ct);
      }
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

  public sealed override async Task<Result> RemoveAndDeleteAsync(TStoreArgs? args, CancellationToken ct)
  {
    var ret = await base.RemoveAndDeleteAsync(args, ct);
    if (ret.IsOk)
    {
      await ClearStateAsync(ct);
    }
    return ret;
  }

  public sealed override async Task SetAsync(
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    await base.SetAsync(value, ct, options);
    await WriteStateAsync(ct);
  }

  public sealed override async Task<Result<TValue>> SetAndWriteAsync(
    TStoreArgs? args,
    TValue value,
    CancellationToken ct,
    CacheGrainEntryOptions? options = null)
  {
    var ret = await base.SetAndWriteAsync(args, value, ct, options);
    if (ret.IsOk)
    {
      await WriteStateAsync(ct);
    }
    return ret;
  }

  public sealed override async Task<Result<TValue>> GetAsync(CancellationToken ct)
  {
    var ret = await base.GetAsync(ct);
    if (ret.IsOk)
    {
      // Only write state if we have sliding expiration, as absolute expiration does not change on access
      if (HasSlidingExpiration)
      {
        await WriteStateAsync(ct);
      }
    }
    else
    {
      await ClearStateAsync(ct);
    }
    return ret;
  }

  public sealed override Task<Result<TValue>> PeekAsync(CancellationToken ct)
  {
    return base.PeekAsync(ct);
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

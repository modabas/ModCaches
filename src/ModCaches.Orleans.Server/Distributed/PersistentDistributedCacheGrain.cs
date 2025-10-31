using System.Collections.Immutable;
using ModCaches.Orleans.Abstractions.Common;
using ModCaches.Orleans.Abstractions.Distributed;
using ModCaches.Orleans.Server.Common;

namespace ModCaches.Orleans.Server.Distributed;

internal class PersistentDistributedCacheGrain : BaseDistributedCacheGrain, IPersistentDistributedCacheGrain
{
  private bool _stateCleared = false;
  private readonly IPersistentState<DistributedCacheState> _persistentState;

  public PersistentDistributedCacheGrain(TimeProvider timeProvider,
    [PersistentState(nameof(PersistentDistributedCacheGrain))] IPersistentState<DistributedCacheState> persistentState)
    : base(timeProvider)
  {
    _persistentState = persistentState;
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    await base.OnActivateAsync(cancellationToken);
    if (_persistentState.RecordExists &&
      _persistentState.State.LastAccessed > DateTimeOffset.MinValue)
    {
      _cacheEntry = new CacheEntry<ImmutableArray<byte>>(
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
      if (_cacheEntry is null ||
        !_cacheEntry.TryPeekValue(_timeProviderFunc, out _, out _))
      {
        await ClearStateAsync(cancellationToken);
      }
    }
    await base.OnDeactivateAsync(reason, cancellationToken);
  }

  public override async Task<ImmutableArray<byte>?> GetAsync(CancellationToken ct)
  {
    var ret = await base.GetAsync(ct);
    if (ret is null)
    {
      await ClearStateAsync(ct);
    }
    else
    {
      // Only write state if we have sliding expiration, as absolute expiration does not change on access
      if (HasSlidingExpiration)
      {
        await WriteStateAsync(ct);
      }
    }
    return ret;
  }

  public override async Task SetAsync(ImmutableArray<byte> value, CacheEntryOptions options, CancellationToken ct)
  {
    await base.SetAsync(value, options, ct);
    await WriteStateAsync(ct);
  }

  public override async Task RemoveAsync(CancellationToken ct)
  {
    await base.RemoveAsync(ct);
    await ClearStateAsync(ct);
  }

  public override async Task<bool> RefreshAsync(CancellationToken ct)
  {
    var ret = await base.RefreshAsync(ct);
    if (ret)
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

  private async Task WriteStateAsync(CancellationToken ct)
  {
    //This is the expected case where we have a valid cache entry to write
    if (_cacheEntry is not null)
    {
      _persistentState.State = _cacheEntry.ToState();
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

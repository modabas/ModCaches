using System.Collections.Immutable;
using ModCaches.OrleansCaches.Common;
using Orleans;
using Orleans.Runtime;

namespace ModCaches.OrleansCaches.Distributed;

internal class PersistentDistributedCacheGrain : Grain, IPersistentDistributedCacheGrain
{
  private CacheEntry<ImmutableArray<byte>>? _cacheEntry;
  private readonly Func<DateTimeOffset> _timeProviderFunc;

  private IPersistentState<DistributedCacheState> _persistentState;

  public PersistentDistributedCacheGrain(TimeProvider timeProvider,
    IPersistentState<DistributedCacheState> persistentState)
  {
    _timeProviderFunc = () => timeProvider.GetUtcNow();
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
    if (_cacheEntry is null ||
      !_cacheEntry.TryGetValue(_timeProviderFunc, out _, out _))
    {
      await ClearStateAsync(cancellationToken);
    }
    await base.OnDeactivateAsync(reason, cancellationToken);
  }

  public async Task<ImmutableArray<byte>?> GetAsync(CancellationToken ct)
  {
    if (_cacheEntry?.TryGetValue(_timeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      await WriteStateAsync(ct);
      return value;
    }
    await RemoveAsync(ct);
    return null;
  }

  public async Task SetAsync(ImmutableArray<byte> value, CacheEntryOptions options, CancellationToken ct)
  {
    _cacheEntry = new CacheEntry<ImmutableArray<byte>>(value, options, _timeProviderFunc);
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    if (_cacheEntry.TryGetExpiresIn(_timeProviderFunc, out var expiresIn))
    {
      DelayDeactivation(expiresIn.Value);
    }
    await WriteStateAsync(ct);
  }

  public Task RemoveAsync(CancellationToken ct)
  {
    _cacheEntry = null; // Remove the cache entry
    DeactivateOnIdle(); // Deactivate the grain after removing the value
    return Task.CompletedTask;
  }

  public async Task RefreshAsync(CancellationToken ct)
  {
    if (_cacheEntry is null ||
      !_cacheEntry.TryGetValue(_timeProviderFunc, out _, out var expiresIn))
    {
      await RemoveAsync(ct);
      return;
    }
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    DelayDeactivation(expiresIn.Value);
    await WriteStateAsync(ct);
    return;
  }

  private async Task WriteStateAsync(CancellationToken ct)
  {
    _persistentState.State = _cacheEntry!.ToState();
    await _persistentState.WriteStateAsync(ct);
  }

  private async Task ClearStateAsync(CancellationToken ct)
  {
    if (_persistentState.RecordExists)
    {
      await _persistentState.ClearStateAsync(ct);
    }
  }
}

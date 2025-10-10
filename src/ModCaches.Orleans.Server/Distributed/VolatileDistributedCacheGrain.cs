using System.Collections.Immutable;
using ModCaches.Orleans.Abstractions.Common;
using ModCaches.Orleans.Abstractions.Distributed;
using ModCaches.Orleans.Server.Common;

namespace ModCaches.Orleans.Server.Distributed;

internal class VolatileDistributedCacheGrain : Grain, IVolatileDistributedCacheGrain
{
  private CacheEntry<ImmutableArray<byte>>? _cacheEntry;
  private readonly Func<DateTimeOffset> _timeProviderFunc;

  public VolatileDistributedCacheGrain(TimeProvider timeProvider)
  {
    _timeProviderFunc = () => timeProvider.GetUtcNow();
  }

  public async Task<ImmutableArray<byte>?> GetAsync(CancellationToken ct)
  {
    if (_cacheEntry?.TryGetValue(_timeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return value;
    }
    await RemoveAsync(ct);
    return null;
  }

  public Task SetAsync(ImmutableArray<byte> value, CacheEntryOptions options, CancellationToken ct)
  {
    _cacheEntry = new CacheEntry<ImmutableArray<byte>>(value, options, _timeProviderFunc);
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    if (_cacheEntry.TryGetExpiresIn(_timeProviderFunc, out var expiresIn))
    {
      DelayDeactivation(expiresIn.Value);
    }
    return Task.CompletedTask;
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
    return;
  }
}

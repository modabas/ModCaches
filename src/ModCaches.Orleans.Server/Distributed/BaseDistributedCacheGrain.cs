using System.Collections.Immutable;
using ModCaches.Orleans.Abstractions.Common;
using ModCaches.Orleans.Abstractions.Distributed;
using ModCaches.Orleans.Server.Common;

namespace ModCaches.Orleans.Server.Distributed;

internal abstract class BaseDistributedCacheGrain : BaseGrain, IBaseDistributedCacheGrain
{
  protected CacheEntry<ImmutableArray<byte>>? CacheEntry { get; set; }
  protected Func<DateTimeOffset> TimeProviderFunc { get; init; }

  protected bool HasSlidingExpiration =>
    CacheEntry?.HasSlidingExpiration ?? false;

  public BaseDistributedCacheGrain(TimeProvider timeProvider)
  {
    TimeProviderFunc = () => timeProvider.GetUtcNow();
  }

  public virtual Task<ImmutableArray<byte>?> GetAsync(CancellationToken ct)
  {
    if (CacheEntry?.TryGetValue(TimeProviderFunc, out var value, out var expiresIn) == true)
    {
      DelayDeactivation(expiresIn.Value);
      return Task.FromResult<ImmutableArray<byte>?>(value);
    }
    RemoveInternal();
    return Task.FromResult<ImmutableArray<byte>?>(null);
  }

  public virtual Task SetAsync(ImmutableArray<byte> value, CacheEntryOptions options, CancellationToken ct)
  {
    CacheEntry = new CacheEntry<ImmutableArray<byte>>(value, options, TimeProviderFunc);
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    if (CacheEntry.TryGetExpiresIn(TimeProviderFunc, out var expiresIn))
    {
      DelayDeactivation(expiresIn.Value);
    }
    return Task.CompletedTask;
  }

  public virtual Task RemoveAsync(CancellationToken ct)
  {
    RemoveInternal();
    return Task.CompletedTask;
  }

  public virtual Task<bool> RefreshAsync(CancellationToken ct)
  {
    if (CacheEntry is null ||
      !CacheEntry.TryGetValue(TimeProviderFunc, out _, out var expiresIn))
    {
      RemoveInternal();
      return Task.FromResult(false);
    }
    // Delay deactivation to ensure it remains active while it has a valid cache entry
    DelayDeactivation(expiresIn.Value);
    return Task.FromResult(true);
  }

  private void RemoveInternal()
  {
    CacheEntry = null; // Remove the cache entry
    ResetDeactivation(); // Reset deactivation to default behavior
    return;
  }
}

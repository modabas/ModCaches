using System.Collections.Immutable;
using ModCaches.OrleansCaches.Common;
using Orleans;

namespace ModCaches.OrleansCaches.Distributed;

internal interface IVolatileDistributedCacheGrain : IGrainWithStringKey
{
  Task<ImmutableArray<byte>?> GetAsync(CancellationToken ct);
  Task SetAsync(ImmutableArray<byte> value, CacheEntryOptions options, CancellationToken ct);
  Task RemoveAsync(CancellationToken ct);
  Task RefreshAsync(CancellationToken ct);
}

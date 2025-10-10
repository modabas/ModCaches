using System.Collections.Immutable;
using ModCaches.Orleans.Abstractions.Common;

namespace ModCaches.Orleans.Abstractions.Distributed;

internal interface IVolatileDistributedCacheGrain : IGrainWithStringKey
{
  Task<ImmutableArray<byte>?> GetAsync(CancellationToken ct);
  Task SetAsync(ImmutableArray<byte> value, CacheEntryOptions options, CancellationToken ct);
  Task RemoveAsync(CancellationToken ct);
  Task RefreshAsync(CancellationToken ct);
}

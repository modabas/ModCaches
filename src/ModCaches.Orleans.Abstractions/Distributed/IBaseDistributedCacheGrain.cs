using System.Collections.Immutable;
using ModCaches.Orleans.Abstractions.Common;

namespace ModCaches.Orleans.Abstractions.Distributed;

internal interface IBaseDistributedCacheGrain : IGrainWithStringKey
{
  Task<ImmutableArray<byte>?> GetAsync(CancellationToken ct);
  Task SetAsync(ImmutableArray<byte> value, CacheEntryOptions options, CancellationToken ct);
  Task RemoveAsync(CancellationToken ct);
  Task<bool> RefreshAsync(CancellationToken ct);
}

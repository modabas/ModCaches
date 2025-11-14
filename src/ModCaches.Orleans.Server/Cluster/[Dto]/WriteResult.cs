using ModCaches.Orleans.Abstractions.Cluster;

namespace ModCaches.Orleans.Server.Cluster;

public record WriteResult<TValue>(
  TValue Value,
  CacheGrainEntryOptions Options)
  where TValue : notnull;

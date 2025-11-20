using ModCaches.Orleans.Abstractions.Cluster;

namespace ModCaches.Orleans.Server.Cluster;

public record WriteRecord<TValue>(
  TValue Value,
  CacheGrainEntryOptions Options)
  where TValue : notnull;

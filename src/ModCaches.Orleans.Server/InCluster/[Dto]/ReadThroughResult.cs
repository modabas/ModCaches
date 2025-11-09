namespace ModCaches.Orleans.Server.InCluster;

public record ReadThroughResult<TValue>(
  TValue Value,
  CacheGrainEntryOptions Options)
  where TValue : notnull;

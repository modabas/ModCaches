namespace ModCaches.Orleans.Server.InCluster;

public record WriteThroughResult<TValue>(
  TValue Value,
  CacheGrainEntryOptions Options)
  where TValue : notnull;

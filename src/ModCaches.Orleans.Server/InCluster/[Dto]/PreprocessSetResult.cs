namespace ModCaches.Orleans.Server.InCluster;

public record PreprocessSetResult<TValue>(
  TValue Value,
  CacheGrainEntryOptions Options)
  where TValue : notnull;

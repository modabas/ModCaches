namespace ModCaches.Orleans.Server.InCluster;

public record GenerateEntryResult<TValue>(
  TValue Value,
  CacheGrainEntryOptions Options) 
  where TValue : notnull;

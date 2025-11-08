namespace ModCaches.Orleans.Server.InCluster;

[GenerateSerializer]
public record TryPeekResult<TValue>(
  bool IsFound,
  TValue? Value)
  where TValue : notnull;

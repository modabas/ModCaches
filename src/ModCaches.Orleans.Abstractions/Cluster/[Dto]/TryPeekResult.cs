namespace ModCaches.Orleans.Abstractions.Cluster;

[GenerateSerializer]
public record TryPeekResult<TValue>(
  bool IsFound,
  TValue? Value)
  where TValue : notnull;

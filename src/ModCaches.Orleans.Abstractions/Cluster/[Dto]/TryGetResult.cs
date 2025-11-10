namespace ModCaches.Orleans.Abstractions.Cluster;

[GenerateSerializer]
public record TryGetResult<TValue>(
  bool IsFound,
  TValue? Value)
  where TValue : notnull;

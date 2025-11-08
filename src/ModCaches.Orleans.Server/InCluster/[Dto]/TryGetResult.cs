namespace ModCaches.Orleans.Server.InCluster;

[GenerateSerializer]
public record TryGetResult<TValue>(
  bool IsFound,
  TValue? Value)
  where TValue : notnull;

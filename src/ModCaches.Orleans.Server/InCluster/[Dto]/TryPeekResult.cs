namespace ModCaches.Orleans.Server.InCluster;

[GenerateSerializer]
public record TryPeekResult<TValue>(
  bool Found,
  TValue? Value)
  where TValue : notnull;

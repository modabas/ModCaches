namespace ModCaches.Orleans.Server.InCluster;

[GenerateSerializer]
public record TryGetResult<TValue>(
  bool Found,
  TValue? Value)
  where TValue : notnull;

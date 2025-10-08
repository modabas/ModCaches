namespace ModCaches.ExtendedDistributedCache;

internal class DefaultDistributedCacheSerializer : IDistributedCacheSerializer
{
  public ValueTask<Memory<byte>> SerializeAsync<T>(T value, CancellationToken ct)
  {
    var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);
    return new(bytes);
  }

  public ValueTask<T?> DeserializeAsync<T>(Memory<byte> bytes, CancellationToken ct)
  {
    if (bytes.Length == 0)
    {
      return new((T?)default);
    }
    var value = System.Text.Json.JsonSerializer.Deserialize<T>(bytes.Span);
    return new(value);
  }
}

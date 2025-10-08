namespace ModCaches.ExtendedDistributedCache;

public interface IDistributedCacheSerializer
{
  ValueTask<Memory<byte>> SerializeAsync<T>(T value, CancellationToken ct);
  ValueTask<T?> DeserializeAsync<T>(Memory<byte> bytes, CancellationToken ct);
}

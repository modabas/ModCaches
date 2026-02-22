using ModCaches.Orleans.Abstractions.Cluster;

namespace ModCaches.Orleans.Server.Cluster;

/// <summary>
/// Represents the response from write to store method. Contains value to be stored in cache and associated entry options.
/// </summary>
/// <remarks>Encapsulates both the data and configuration for a cache entry. The options
/// parameter allows customization of cache entry handling, i.e. expiration.</remarks>
/// <typeparam name="TValue">The type of the value to be written to the cache. Must not non-null.</typeparam>
/// <param name="Value">The value to be stored in the cache entry.</param>
/// <param name="Options">The options that configure the behavior of the cache entry, i.e. expiration.</param>
public record WriteRecord<TValue>(
  TValue Value,
  CacheGrainEntryOptions Options)
  where TValue : notnull;

public static class WriteRecord
{
  /// <summary>
  /// Creates a new instance of the <see cref="WriteRecord{TValue}"/> class using the specified value and cache entry options.
  /// </summary>
  /// <typeparam name="TValue">The type of the value to be stored in the cache. Must be non-null.</typeparam>
  /// <param name="value">The value to be stored in the cache record. Cannot be null.</param>
  /// <param name="options">The cache entry options that configure the behavior of the cache record.</param>
  /// <returns>A <see cref="WriteRecord{TValue}"/> instance containing the specified value and cache entry options.</returns>
  public static WriteRecord<TValue> From<TValue>(
    TValue value,
    CacheGrainEntryOptions options)
    where TValue : notnull
  {
    return new WriteRecord<TValue>(value, options);
  }
}

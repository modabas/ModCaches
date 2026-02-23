using ModCaches.Orleans.Abstractions.Cluster;
using ModResults;

namespace ModCaches.Orleans.Server.Cluster;

/// <summary>
/// Represents the response from create from store method. Contains value to be stored in cache and associated entry options.
/// </summary>
/// <remarks>Encapsulates both the data and configuration for a cache entry. The options
/// parameter allows customization of cache entry handling, i.e. expiration.</remarks>
/// <typeparam name="TValue">The type of the value to be written to the cache. Must not non-null.</typeparam>
/// <param name="Value">The value to be stored in the cache entry.</param>
/// <param name="Options">The options that configure the behavior of the cache entry, i.e. expiration.</param>
public record CacheGrainEntry<TValue>(
  TValue Value,
  CacheGrainEntryOptions Options)
  where TValue : notnull;

public static class CacheGrainEntry
{
  /// <summary>
  /// Creates a new instance of the <see cref="CacheGrainEntry{TValue}"/> class using the specified value and cache entry options.
  /// </summary>
  /// <typeparam name="TValue">The type of the value to be stored in the cache entry. Must be non-null.</typeparam>
  /// <param name="value">The value to be stored in the cache entry.</param>
  /// <param name="options">The options used to configure the cache entry.</param>
  /// <returns>A <see cref="CacheGrainEntry{TValue}"/> instance containing the specified value and cache entry options.</returns>
  public static CacheGrainEntry<TValue> Create<TValue>(
    TValue value,
    CacheGrainEntryOptions options)
    where TValue : notnull
  {
    return new CacheGrainEntry<TValue>(value, options);
  }

  /// <summary>
  /// Creates a successful result containing a new cache entry initialized with the specified value and options.
  /// </summary>
  /// <typeparam name="TValue">The type of the value to be stored in the cache entry. Must be non-null.</typeparam>
  /// <param name="value">The value to be stored in the cache entry.</param>
  /// <param name="options">The options used to configure the cache entry.</param>
  /// <returns>A successful result containing a new <see cref="CacheGrainEntry{TValue}"/> instance initialized with the specified
  /// value and options.</returns>
  public static Result<CacheGrainEntry<TValue>> CreateResult<TValue>(
    TValue value,
    CacheGrainEntryOptions options)
    where TValue : notnull
  {
    return Result.Ok(Create(value, options));
  }
}

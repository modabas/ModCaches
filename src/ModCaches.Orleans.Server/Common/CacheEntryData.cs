namespace ModCaches.Orleans.Server.Common;

internal record CacheEntryData<T>(
  T Value,
  DateTimeOffset? AbsoluteExpiration,
  TimeSpan? SlidingExpiration,
  DateTimeOffset LastAccessed)
  where T : notnull;

namespace ModCaches.Orleans.Abstractions.Common;

[GenerateSerializer]
internal record CacheEntryOptions(DateTimeOffset? AbsoluteExpiration, TimeSpan? AbsoluteExpirationRelativeToNow, TimeSpan? SlidingExpiration);

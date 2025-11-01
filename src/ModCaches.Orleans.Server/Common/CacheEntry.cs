using System.Diagnostics.CodeAnalysis;
using ModCaches.Orleans.Abstractions.Common;

namespace ModCaches.Orleans.Server.Common;

internal class CacheEntry<T> where T : notnull
{
  private readonly T _value;
  private readonly DateTimeOffset? _absoluteExpiration;
  private readonly TimeSpan? _slidingExpiration;
  private DateTimeOffset _lastAccessed;

  public bool HasSlidingExpiration => _slidingExpiration.HasValue;

  public CacheEntry(T value,
    CacheEntryOptions options,
    Func<DateTimeOffset> timeProviderFunc)
  {
    _value = value;
    var now = timeProviderFunc();
    _lastAccessed = now;

    var absoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow < TimeSpan.Zero ?
      TimeSpan.Zero : options.AbsoluteExpirationRelativeToNow;

    if (absoluteExpirationRelativeToNow is not null)
    {
      _absoluteExpiration = now + absoluteExpirationRelativeToNow;
    }
    if (options.AbsoluteExpiration is not null)
    {
      _absoluteExpiration = _absoluteExpiration is null ?
        options.AbsoluteExpiration :
        _absoluteExpiration < options.AbsoluteExpiration ?
          _absoluteExpiration : options.AbsoluteExpiration;
    }
    _slidingExpiration = options.SlidingExpiration < TimeSpan.Zero ?
      TimeSpan.Zero : options.SlidingExpiration;
  }

  internal CacheEntry(
    T value,
    DateTimeOffset? absoluteExpiration,
    TimeSpan? slidingExpiration,
    DateTimeOffset lastAccessed)
  {
    _value = value;
    _absoluteExpiration = absoluteExpiration;
    _slidingExpiration = slidingExpiration;
    _lastAccessed = lastAccessed;
  }

  private bool TryGetExpiresIn(DateTimeOffset now, [NotNullWhen(true)] out TimeSpan? expiresIn)
  {
    var absoluteExpiration = TimeSpan.MaxValue;
    var slidingExpiration = TimeSpan.MaxValue;
    if (_absoluteExpiration.HasValue)
    {
      absoluteExpiration = _absoluteExpiration.Value - now;
    }
    if (_slidingExpiration.HasValue)
    {
      slidingExpiration = _slidingExpiration.Value - (now - _lastAccessed);
    }
    expiresIn = absoluteExpiration < slidingExpiration
      ? absoluteExpiration
      : slidingExpiration;
    if (expiresIn < TimeSpan.Zero)
    {
      expiresIn = null;
      return false;
    }
    return true;
  }

  public bool TryGetValue(
    Func<DateTimeOffset> timeProviderFunc,
    [NotNullWhen(true)] out T? value,
    [NotNullWhen(true)] out TimeSpan? expiresIn)
  {
    var now = timeProviderFunc();
    if (!TryGetExpiresIn(now, out _))
    {
      value = default;
      expiresIn = null;
      return false;
    }
    _lastAccessed = now;
    if (TryGetExpiresIn(now, out expiresIn))
    {
      value = _value;
      return true;
    }
    value = default;
    expiresIn = null;
    return false;
  }

  public bool TryPeekValue(
    Func<DateTimeOffset> timeProviderFunc,
    [NotNullWhen(true)] out T? value,
    [NotNullWhen(true)] out TimeSpan? expiresIn)
  {
    var now = timeProviderFunc();
    if (TryGetExpiresIn(now, out expiresIn))
    {
      value = _value;
      return true;
    }
    value = default;
    expiresIn = null;
    return false;
  }

  public bool TryGetExpiresIn(Func<DateTimeOffset> timeProviderFunc, [NotNullWhen(true)] out TimeSpan? expiresIn)
  {
    var now = timeProviderFunc();
    return TryGetExpiresIn(now, out expiresIn);
  }

  internal CacheEntryData<T> GetStoredData()
  {
    return new(_value, _absoluteExpiration, _slidingExpiration, _lastAccessed);
  }
}

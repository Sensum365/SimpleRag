namespace SimpleRag;

/// <summary>
/// How to cache search embedding (or the data itself)
/// <param name="Type">The type of cache to use</param>
/// <param name="CacheDuration">How long should the cache be</param>
/// </summary>
public record SearchCachingStrategy(SearchCachingStrategyType Type, TimeSpan CacheDuration);
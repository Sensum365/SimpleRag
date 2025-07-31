namespace SimpleRag;

/// <summary>
/// The type of Caching
/// </summary>
public enum SearchCachingStrategyType
{
    /// <summary>
    /// Cache the Embedding used for caching
    /// </summary>
    CacheEmbedding,

    /// <summary>
    /// Cache the Result of a search
    /// </summary>
    CacheSearchResult
}
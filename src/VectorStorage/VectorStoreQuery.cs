using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.VectorData;
using Polly.Utilities;
using SimpleRag.VectorStorage.Models;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SimpleRag.VectorStorage;

/// <summary>
/// Provides query operations against the vector store.
/// </summary>
[PublicAPI]
public class VectorStoreQuery(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, VectorStore vectorStore, VectorStoreConfiguration vectorStoreConfiguration, IMemoryCache memoryCache) : IVectorStoreQuery
{
    private bool _creationEnsured;

    private async Task<VectorStoreCollection<string, VectorEntity>> GetCollectionAndEnsureItExist(CancellationToken cancellationToken = default)
    {
        VectorStoreCollection<string, VectorEntity> collection = vectorStore.GetCollection<string, VectorEntity>(vectorStoreConfiguration.CollectionName);
        if (_creationEnsured)
        {
            return collection;
        }

        await collection.EnsureCollectionExistsAsync(cancellationToken);
        _creationEnsured = true;
        return collection;
    }

    /// <summary>
    /// Searches the vector store.
    /// <param name="searchQuery">The input search query</param>
    /// <param name="numberOfRecordsBack">The max number of records back (can be overwritten by the general vector-store configuration)</param>
    /// <param name="filter">The Filter to apply to the search</param>
    /// <param name="thresholdSimilarityScoreToReturn">The Threshold Score to return (Not called min/max as some vector stores see low numbers as most similar while others see high numbers as most similar. The system will check that on the fly and use the indicated score as min/max)</param>
    /// <param name="cachingStrategy">How to do caching in search</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>The SearchResult</returns>
    /// </summary>
    public async Task<SearchResult> SearchAsync(string searchQuery, int numberOfRecordsBack, Expression<Func<VectorEntity, bool>>? filter, double? thresholdSimilarityScoreToReturn = null, SearchCachingStrategy? cachingStrategy = null, CancellationToken cancellationToken = default)
    {
        string? cacheKey = null;
        if (cachingStrategy?.Type == SearchCachingStrategyType.CacheSearchResult)
        {
            cacheKey = searchQuery + GetCacheKeyFromExpression(filter) + numberOfRecordsBack + thresholdSimilarityScoreToReturn;
            memoryCache.TryGetValue(cacheKey, out SearchResult? cachedSearchResult);
            if (cachedSearchResult != null)
            {
                return cachedSearchResult;
            }
        }

        VectorStoreCollection<string, VectorEntity> collection = await GetCollectionAndEnsureItExist(cancellationToken);
        VectorSearchOptions<VectorEntity> vectorSearchOptions = new()
        {
            IncludeVectors = false
        };
        if (filter != null)
        {
            vectorSearchOptions.Filter = filter;
        }

        if (vectorStoreConfiguration.MaxRecordSearch.HasValue && numberOfRecordsBack > vectorStoreConfiguration.MaxRecordSearch.Value)
        {
            numberOfRecordsBack = vectorStoreConfiguration.MaxRecordSearch.Value;
        }

        Embedding<float>? embedding;
        if (cachingStrategy?.Type == SearchCachingStrategyType.CacheEmbedding)
        {
            memoryCache.TryGetValue(searchQuery, out embedding);
            if (embedding == null)
            {
                embedding = await embeddingGenerator.GenerateAsync(searchQuery, cancellationToken: cancellationToken);
                memoryCache.Set(searchQuery, embedding, cachingStrategy.CacheDuration);
            }
        }
        else
        {
            embedding = await embeddingGenerator.GenerateAsync(searchQuery, cancellationToken: cancellationToken);
        }

        List<VectorSearchResult<VectorEntity>> result = [];
        await foreach (VectorSearchResult<VectorEntity> searchResult in collection.SearchAsync(embedding, numberOfRecordsBack, vectorSearchOptions, cancellationToken))
        {
            result.Add(searchResult);
        }

        if (thresholdSimilarityScoreToReturn.HasValue && result.Count > 1) //In the case of only 1 record back it is impossible to know if scores go from low to high or reverse so we will always return that single value in that case
        {
            double? firstScore = result.First().Score;
            double? lastScore = result.Last().Score;
            if (firstScore < lastScore)
            {
                //low to high
                result = result.Where(x => x.Score < thresholdSimilarityScoreToReturn.Value).ToList();
            }
            else if (lastScore < firstScore)
            {
                //high to low
                result = result.Where(x => x.Score > thresholdSimilarityScoreToReturn.Value).ToList();
            }
            // ReSharper disable once RedundantIfElseBlock
            else
            {
                //In the unlikely event that the first and the last score are the same we can't know if it is a low to high or high to low range so we include everything
            }
        }

        SearchResult toReturn = new()
        {
            Entities = result.ToArray()
        };
        if (cachingStrategy?.Type == SearchCachingStrategyType.CacheSearchResult && !string.IsNullOrWhiteSpace(cacheKey))
        {
            memoryCache.Set(cacheKey, toReturn, cachingStrategy.CacheDuration);
        }

        return toReturn;
    }

    /// <summary>
    /// Retrieves existing records matching the filter.
    /// </summary>
    public async Task<VectorEntity[]> GetExistingAsync(Expression<Func<VectorEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
    {
        List<VectorEntity> result = [];
        VectorStoreCollection<string, VectorEntity> collection = await GetCollectionAndEnsureItExist(cancellationToken);
        await collection.EnsureCollectionExistsAsync(cancellationToken);

        // ReSharper disable once EqualExpressionComparison
        Expression<Func<VectorEntity, bool>> filterToUse = entity => entity.Id == entity.Id;
        if (filter != null)
        {
            filterToUse = filter;
        }

        await foreach (VectorEntity entity in collection.GetAsync(filterToUse, int.MaxValue, new FilteredRecordRetrievalOptions<VectorEntity>
                       {
                           IncludeVectors = false
                       }, cancellationToken))
        {
            result.Add(entity);
        }

        return result.ToArray();
    }

    private static string GetCacheKeyFromExpression<T>(Expression<Func<T, bool>>? filter)
    {
        if (filter == null)
        {
            return "NO_FILTER";
        }

        // 1. Get a string representation of the expression.
        // This is a good starting point as it captures the tree structure.
        string expressionString = filter.ToString();

        // 2. Normalize the string to ensure consistency.
        string normalizedExpression = expressionString.Trim().ToLowerInvariant();

        // 3. Hash the normalized string to get a fixed-length key.
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalizedExpression));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
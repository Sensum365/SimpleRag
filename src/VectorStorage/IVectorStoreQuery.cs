using JetBrains.Annotations;
using SimpleRag.VectorStorage.Models;
using System.Linq.Expressions;

namespace SimpleRag.VectorStorage;

/// <summary>
/// Represent a VectorStoreQuery
/// </summary>
[PublicAPI]
public interface IVectorStoreQuery
{
    /// <summary>
    /// Searches the vector store.
    /// </summary>
    Task<SearchResult> SearchAsync(string searchQuery, int numberOfRecordsBack, Expression<Func<VectorEntity, bool>>? filter, double? thresholdSimilarityScoreToReturn = null, SearchCachingStrategy? cachingStrategy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves existing records matching the filter.
    /// </summary>
    Task<VectorEntity[]> GetExistingAsync(Expression<Func<VectorEntity, bool>>? filter = null, CancellationToken cancellationToken = default);
}
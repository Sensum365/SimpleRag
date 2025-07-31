using System.Linq.Expressions;
using JetBrains.Annotations;
using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;

namespace SimpleRag;

/// <summary>
/// Performs search operations against the vector store.
/// </summary>
[PublicAPI]
public class Search(IVectorStoreQuery vectorStoreQuery)
{
    /// <summary>
    /// Search using the provided options
    /// </summary>
    /// <param name="options">The Search options</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>The result of the search</returns>
    public async Task<SearchResult> SearchAsync(SearchOptions options, CancellationToken cancellationToken = default)
    {
        string collectionIdAsString = options.CollectionId.Value;
        Expression<Func<VectorEntity, bool>>? filter;
        switch (options)
        {
            case { SourceKind: not null, SourceId: not null }:
            {
                filter = x => x.SourceCollectionId == collectionIdAsString && x.SourceKind == options.SourceKind && x.SourceId == options.SourceId.Value.Value;
                break;
            }
            case { SourceKind: not null }:
            {
                filter = x => x.SourceCollectionId == collectionIdAsString && x.SourceKind == options.SourceKind;
                break;
            }
            case { SourceId: not null }:
            {
                filter = x => x.SourceCollectionId == collectionIdAsString && x.SourceId == options.SourceId.Value.Value;
                break;
            }
            default:
                filter = x => x.SourceCollectionId == collectionIdAsString;
                break;
        }

        return await vectorStoreQuery.SearchAsync(options.SearchQuery, options.NumberOfRecordsBack, filter, options.ThresholdSimilarityScoreToReturn, options.SearchCachingStrategy, cancellationToken);
    }

    /// <summary>
    /// Search using the provided advanced filter options.
    /// <param name="options">The Advanced Search Options</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>The result of the search</returns>
    /// </summary>
    public async Task<SearchResult> SearchAsync(SearchOptionsAdvanced options, CancellationToken cancellationToken = default)
    {
        return await vectorStoreQuery.SearchAsync(options.SearchQuery, options.NumberOfRecordsBack, options.Filter, options.ThresholdSimilarityScoreToReturn, cancellationToken: cancellationToken);
    }
}
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
    /// <returns>The result of the search</returns>
    public async Task<SearchResult> SearchAsync(SearchOptions options)
    {
        string collectionIdAsString = options.CollectionId.Value;
        Expression<Func<VectorEntity, bool>>? filter;
        switch (options)
        {
            case { SourceKind: not null, SourceId: not null }:
            {
                string sourceIdAsString = options.SourceId.Value.Value;
                string sourceKindAsString = options.SourceKind;
                filter = x => x.SourceCollectionId == collectionIdAsString && x.SourceKind == sourceKindAsString && x.SourceId == sourceIdAsString;
                break;
            }
            case { SourceKind: not null }:
            {
                string sourceKindAsString = options.SourceKind;
                filter = x => x.SourceCollectionId == collectionIdAsString && x.SourceKind == sourceKindAsString;
                break;
            }
            case { SourceId: not null }:
            {
                string sourceIdAsString = options.SourceId.Value.Value;
                filter = x => x.SourceCollectionId == collectionIdAsString && x.SourceId == sourceIdAsString;
                break;
            }
            default:
                filter = x => x.SourceCollectionId == collectionIdAsString;
                break;
        }

        return await vectorStoreQuery.SearchAsync(options.SearchQuery, options.NumberOfRecordsBack, filter);
    }

    /// <summary>
    /// Search using the provided advanced filter options.
    /// <param name="options">The Advanced Search Options</param>
    /// <returns>The result of the search</returns>
    /// </summary>
    public async Task<SearchResult> SearchAsync(SearchOptionsAdvanced options)
    {
        return await vectorStoreQuery.SearchAsync(options.SearchQuery, options.NumberOfRecordsBack, options.Filter);
    }
}
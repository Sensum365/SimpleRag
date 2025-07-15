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
    /// Executes a search using the provided options.
    /// </summary>
    public async Task<SearchResult> SearchAsync(SearchOptions options)
    {
        return await vectorStoreQuery.SearchAsync(options.SearchQuery, options.NumberOfRecordsBack, options.Filter);
    }
}
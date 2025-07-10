using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;
using System.Linq.Expressions;

namespace SimpleRag;

/// <summary>
/// Performs search operations against the vector store.
/// </summary>
public class Search(VectorStoreQuery vectorStoreQuery)
{
    /// <summary>
    /// Executes a search using the provided options.
    /// </summary>
    public async Task<SearchResult> SearchAsync(SearchOptions options)
    {
        return await vectorStoreQuery.SearchAsync(options.SearchQuery, options.NumberOfRecordsBack, options.Filter);
    }
}
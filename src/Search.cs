using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;
using System.Linq.Expressions;

namespace SimpleRag;

public class Search(VectorStoreQuery vectorStoreQuery)
{
    public async Task<SearchResult> SearchAsync(SearchOptions options)
    {
        return await vectorStoreQuery.SearchAsync(options.SearchQuery, options.NumberOfRecordsBack, options.Filter);
    }
}
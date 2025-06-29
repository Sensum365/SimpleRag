using System.Linq.Expressions;
using Microsoft.Extensions.VectorData;
using SimpleRag.VectorStorage.Models;

namespace SimpleRag.VectorStorage;

public class VectorStoreQuery(VectorStore vectorStore, VectorStoreConfiguration vectorStoreConfiguration)
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


    public async Task<SearchResult> SearchAsync(string searchQuery, int numberOfRecordsBack, Expression<Func<VectorEntity, bool>>? filter)
    {
        VectorStoreCollection<string, VectorEntity> collection = await GetCollectionAndEnsureItExist();
        await collection.EnsureCollectionExistsAsync();
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

        List<VectorSearchResult<VectorEntity>> result = [];
        await foreach (VectorSearchResult<VectorEntity> searchResult in collection.SearchAsync(searchQuery, numberOfRecordsBack, vectorSearchOptions))
        {
            result.Add(searchResult);
        }

        return new SearchResult
        {
            Entities = result.ToArray()
        };
    }

    public async Task<VectorEntity[]> GetExistingAsync(Expression<Func<VectorEntity, bool>>? filter = null)
    {
        List<VectorEntity> result = [];
        VectorStoreCollection<string, VectorEntity> collection = await GetCollectionAndEnsureItExist();
        await collection.EnsureCollectionExistsAsync();

        Expression<Func<VectorEntity, bool>> filterToUse = entity => true;
        if (filter != null)
        {
            filterToUse = filter;
        }

        await foreach (VectorEntity entity in collection.GetAsync(filterToUse, int.MaxValue, new FilteredRecordRetrievalOptions<VectorEntity>
                       {
                           IncludeVectors = false
                       }))
        {
            result.Add(entity);
        }

        return result.ToArray();
    }
}
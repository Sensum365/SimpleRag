using System.Linq.Expressions;
using Microsoft.Extensions.VectorData;
using SimpleRag.VectorStorage.Models;

namespace SimpleRag.VectorStorage;

/// <summary>
/// Provides query operations against the vector store.
/// </summary>
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


    /// <summary>
    /// Searches the vector store.
    /// </summary>
    public async Task<SearchResult> SearchAsync(string searchQuery, int numberOfRecordsBack, Expression<Func<VectorEntity, bool>>? filter, CancellationToken cancellationToken = default)
    {
        VectorStoreCollection<string, VectorEntity> collection = await GetCollectionAndEnsureItExist(cancellationToken);
        await collection.EnsureCollectionExistsAsync(cancellationToken);
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
        await foreach (VectorSearchResult<VectorEntity> searchResult in collection.SearchAsync(searchQuery, numberOfRecordsBack, vectorSearchOptions, cancellationToken))
        {
            result.Add(searchResult);
        }

        return new SearchResult
        {
            Entities = result.ToArray()
        };
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
}
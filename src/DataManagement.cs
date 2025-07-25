using JetBrains.Annotations;
using SimpleRag.DataSources;
using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;
using System.Linq.Expressions;

namespace SimpleRag;

/// <summary>
/// Do Data Management on existing DataSources
/// </summary>
[PublicAPI]
public class DataManagement(IVectorStoreCommand vectorStoreCommand, IVectorStoreQuery vectorStoreQuery)
{
    /// <summary>
    /// Get everything in a specific Datasource
    /// </summary>
    /// <param name="collectionId">The id of the collection the source is in</param>
    /// <param name="sourceId">The id of the source to delete</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task<VectorEntity[]> GetDataAsync(CollectionId collectionId, SourceId sourceId, CancellationToken cancellationToken = default)
    {
        string collectionIdAsString = collectionId.Value;
        string sourceIdAsString = sourceId.Value;
        return await vectorStoreQuery.GetExistingAsync(entity => entity.SourceCollectionId == collectionIdAsString && entity.SourceId == sourceIdAsString, cancellationToken);
    }

    /// <summary>
    /// Get everything in a specific Datasource Collection (aka every datasource that have the same CollectionId)
    /// </summary>
    /// <param name="collectionId">The CollectionId</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task<VectorEntity[]> GetDataAsync(CollectionId collectionId, CancellationToken cancellationToken = default)
    {
        string collectionIdAsString = collectionId.Value;
        return await vectorStoreQuery.GetExistingAsync(entity => entity.SourceCollectionId == collectionIdAsString, cancellationToken);
    }

    /// <summary>
    /// Get everything that match the filter
    /// </summary>
    /// <param name="filter">The filter for the data to get</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task<VectorEntity[]> GetDataAsync(Expression<Func<VectorEntity, bool>>? filter, CancellationToken cancellationToken = default)
    {
        return await vectorStoreQuery.GetExistingAsync(filter, cancellationToken);
    }

    /// <summary>
    /// Delete everything in a specific Datasource
    /// </summary>
    /// <param name="collectionId">The id of the collection the source is in</param>
    /// <param name="sourceId">The id of the source to delete</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task DeleteSourceDataAsync(CollectionId collectionId, SourceId sourceId, CancellationToken cancellationToken = default)
    {
        string collectionIdAsString = collectionId.Value;
        string sourceIdAsString = sourceId.Value;
        await vectorStoreCommand.DeleteAsync(entity => entity.SourceCollectionId == collectionIdAsString && entity.SourceId == sourceIdAsString, cancellationToken);
    }

    /// <summary>
    /// Delete everything in a specific Datasource Collection (aka every datasource that have the same CollectionId)
    /// </summary>
    /// <param name="collectionId">The CollectionId</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task DeleteCollectionDataAsync(CollectionId collectionId, CancellationToken cancellationToken = default)
    {
        string collectionIdAsString = collectionId.Value;
        await vectorStoreCommand.DeleteAsync(entity => entity.SourceCollectionId == collectionIdAsString, cancellationToken);
    }
}
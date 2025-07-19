using JetBrains.Annotations;
using SimpleRag.VectorStorage;

namespace SimpleRag;

/// <summary>
/// Do Data Management on existing DataSources
/// </summary>
/// <param name="vectorStoreCommand"></param>
[PublicAPI]
public class DataManagement(IVectorStoreCommand vectorStoreCommand)
{
    /// <summary>
    /// Delete everything in a specific Datasource
    /// </summary>
    /// <param name="id">Id of the DataSource</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task DeleteSourceDataAsync(string id, CancellationToken cancellationToken = default)
    {
        await vectorStoreCommand.DeleteAsync(entity => entity.SourceId == id, cancellationToken);
    }

    /// <summary>
    /// Delete everything in a specific Datasource Collection (aka every datasource that have the same CollectionId)
    /// </summary>
    /// <param name="collectionId">The CollectionId</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task DeleteCollectionDataAsync(string collectionId, CancellationToken cancellationToken = default)
    {
        await vectorStoreCommand.DeleteAsync(entity => entity.SourceCollectionId == collectionId, cancellationToken);
    }
}
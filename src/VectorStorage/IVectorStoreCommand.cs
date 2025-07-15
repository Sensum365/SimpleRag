using SimpleRag.DataSources;
using SimpleRag.VectorStorage.Models;
using System.Linq.Expressions;

namespace SimpleRag.VectorStorage;

/// <summary>
/// Represent a VectorStoreCommand
/// </summary>
public interface IVectorStoreCommand
{
    /// <summary>
    /// Inserts or updates the specified entity.
    /// </summary>
    Task UpsertAsync(VectorEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete Entries that match a Filter
    /// </summary>
    /// <param name="filter">The Filter to match (Match >> Delete)</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task DeleteAsync(Expression<Func<VectorEntity, bool>> filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the entities with the specified Ids.
    /// <param name="idsToDelete">Ids to delete</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// </summary>
    Task DeleteAsync(IEnumerable<string> idsToDelete, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sync a new set of VectorEntities with the VectorStore (New/Updated will be Upserted, Unchanged will be skipped and Existing not in the collection will be deleted)
    /// </summary>
    /// <param name="dataSource">The Datasource the entities represent</param>
    /// <param name="vectorEntities">The new set of Entities.</param>
    /// <param name="onProgressNotification">Action to Report notifications</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task SyncAsync(IDataSource dataSource, IEnumerable<VectorEntity> vectorEntities, Action<Notification>? onProgressNotification = null, CancellationToken cancellationToken = default);
}
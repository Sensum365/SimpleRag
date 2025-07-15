using SimpleRag.VectorStorage.Models;
using System.Linq.Expressions;

namespace SimpleRag.VectorStorage;

public interface IVectorStoreCommand
{
    /// <summary>
    /// Inserts or updates the specified entity.
    /// </summary>
    Task UpsertAsync(VectorEntity entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(Expression<Func<VectorEntity, bool>> filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the entities with the specified Ids.
    /// </summary>
    Task DeleteAsync(IEnumerable<string> idsToDelete, CancellationToken cancellationToken = default);
}
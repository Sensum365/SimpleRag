using System.Linq.Expressions;
using SimpleRag.VectorStorage.Models;

namespace SimpleRag.VectorStorage;

public interface IVectorStoreQuery
{
    /// <summary>
    /// Searches the vector store.
    /// </summary>
    Task<SearchResult> SearchAsync(string searchQuery, int numberOfRecordsBack, Expression<Func<VectorEntity, bool>>? filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves existing records matching the filter.
    /// </summary>
    Task<VectorEntity[]> GetExistingAsync(Expression<Func<VectorEntity, bool>>? filter = null, CancellationToken cancellationToken = default);
}
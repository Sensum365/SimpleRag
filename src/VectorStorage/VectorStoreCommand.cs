using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Extensions.VectorData;
using SimpleRag.VectorStorage.Models;

namespace SimpleRag.VectorStorage;

/// <summary>
/// Provides commands for modifying the vector store.
/// </summary>
[PublicAPI]
public class VectorStoreCommand(VectorStore vectorStore, VectorStoreConfiguration vectorStoreConfiguration) : IVectorStoreCommand
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
    /// Inserts or updates the specified entity.
    /// </summary>
    public async Task UpsertAsync(VectorEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = await GetCollectionAndEnsureItExist(cancellationToken);
            await collection.UpsertAsync(entity, cancellationToken);
        }
        catch (Exception e)
        {
            if (e.Message.Contains("This model's maximum context length is"))
            {
                //Too big. Splitting in two recursive until content fit
                int middle = entity.Content.Length / 2;
                string? name = entity.ContentName;
                string part1 = entity.Content.Substring(0, middle);
                string part2 = entity.Content.Substring(middle);
                entity.Content = part1;
                entity.ContentName = name + $" ({Guid.NewGuid()})";
                await UpsertAsync(entity);
                entity.Id = Guid.NewGuid().ToString();
                entity.Content = part2;
                entity.ContentName = name + $" ({Guid.NewGuid()})";
                await UpsertAsync(entity);
            }
            else
            {
                throw;
            }
        }
    }


    /// <summary>
    /// Deletes all entities matching the filter.
    /// </summary>
    public async Task DeleteAsync(Expression<Func<VectorEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        List<string> keysToDelete = [];
        var collection = await GetCollectionAndEnsureItExist(cancellationToken);
        await foreach (VectorEntity entity in collection.GetAsync(filter, int.MaxValue, new FilteredRecordRetrievalOptions<VectorEntity>
                       {
                           IncludeVectors = false
                       }, cancellationToken))
        {
            keysToDelete.Add(entity.Id);
        }

        await DeleteAsync(keysToDelete, cancellationToken);
    }

    /// <summary>
    /// Deletes all entities matching the filter.
    /// </summary>
    public async Task DeleteAsync(IEnumerable<string> idsToDelete, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAndEnsureItExist(cancellationToken);
        await collection.DeleteAsync(idsToDelete, cancellationToken);
    }
}
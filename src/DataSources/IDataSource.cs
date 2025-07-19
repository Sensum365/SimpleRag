using JetBrains.Annotations;

namespace SimpleRag.DataSources;

/// <summary>
/// Representation of a Datasource
/// </summary>
[PublicAPI]
public interface IDataSource
{
    /// <summary>
    /// Gets or sets the identifier of the collection.
    /// </summary>
    string CollectionId { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier of the source.
    /// </summary>
    string Id { get; init; }

    /// <summary>
    /// Ingest the Datasource to the VectorStore
    /// </summary>
    /// <param name="ingestionOptions">Options for Ingestion</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task IngestAsync(IngestionOptions? ingestionOptions, CancellationToken cancellationToken = default);
}
namespace SimpleRag.DataSources;

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

    Task IngestAsync(IngestionOptions? ingestionOptions, CancellationToken cancellationToken = default);
}
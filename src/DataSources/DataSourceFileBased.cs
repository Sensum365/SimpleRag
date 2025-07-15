using SimpleRag.DataProviders.Models;

namespace SimpleRag.DataSources;

/// <summary>
/// Base class for all ingestible data sources.
/// </summary>
public abstract class DataSourceFileBased : IDataSource
{
    /// <summary>
    /// Gets or sets the identifier of the collection.
    /// </summary>
    public required string CollectionId { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier of the source.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether subdirectories are scanned.
    /// </summary>
    public bool Recursive { get; init; } = true;

    /// <summary>
    /// Gets or sets the root path of the source.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets or sets patterns for files to ignore.
    /// </summary>
    public string? FileIgnorePatterns { get; init; }

    /// <summary>
    /// Gets or sets the threshold for ignoring large files.
    /// </summary>
    public int? IgnoreFileIfMoreThanThisNumberOfLines { get; init; }

    /// <summary>
    /// The provider of the source (Local, GitHub, etc.)
    /// </summary>
    public required IFileContentProvider FilesProvider { get; init; }

    /// <summary>
    /// Converts this instance to a <see cref="FileContentSource"/>.
    /// </summary>
    /// <returns>The created file content source.</returns>
    protected FileContentSource AsFileContentSource(string fileExtensionType)
    {
        return new FileContentSource
        {
            FileIgnorePatterns = FileIgnorePatterns,
            Path = Path,
            Recursive = Recursive,
            FileExtensionType = fileExtensionType
        };
    }

    /// <summary>
    /// Ingest the Datasource to the VectorStore
    /// </summary>
    /// <param name="ingestionOptions">Options for Ingestion</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public abstract Task IngestAsync(IngestionOptions? ingestionOptions = null, CancellationToken cancellationToken = default);
}
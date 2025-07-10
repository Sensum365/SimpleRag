namespace SimpleRag.DataSources.Models;

/// <summary>
/// Base class for all ingestible data sources.
/// </summary>
public abstract class DataSource
{
    /// <summary>Gets or sets the identifier of the collection.</summary>
    public required string CollectionId { get; init; }

    /// <summary>Gets or sets the unique identifier of the source.</summary>
    public required string Id { get; init; }

    /// <summary>Gets or sets a value indicating whether subdirectories are scanned.</summary>
    public bool Recursive { get; init; } = true;

    /// <summary>Gets or sets the root path of the source.</summary>
    public required string Path { get; init; }

    /// <summary>Gets or sets patterns for files to ignore.</summary>
    public string? FileIgnorePatterns { get; init; }

    /// <summary>Gets or sets the threshold for ignoring large files.</summary>
    public int? IgnoreFileIfMoreThanThisNumberOfLines { get; init; }
}
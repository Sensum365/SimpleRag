using SimpleRag.DataSources.Models;

namespace SimpleRag.DataSources.Markdown.Models;

/// <summary>
/// Base class for markdown sources.
/// </summary>
public abstract class MarkdownDataSource : DataSource
{
    /// <summary>Gets or sets a value indicating whether HTML comments should be ignored.</summary>
    public bool IgnoreCommentedOutContent { get; init; } = true;

    /// <summary>Gets or sets a value indicating whether image references should be ignored.</summary>
    public bool IgnoreImages { get; init; } = true;

    /// <summary>Gets or sets the line count threshold for chunking files.</summary>
    public int? OnlyChunkIfMoreThanThisNumberOfLines { get; init; } = 25;

    /// <summary>Gets or sets the heading levels to chunk at.</summary>
    public int LevelsToChunk { get; init; } = 2;

    /// <summary>Gets or sets patterns for lines to ignore when chunking.</summary>
    public string? ChunkLineIgnorePatterns { get; init; }

    /// <summary>Gets or sets the minimum size of a chunk in characters.</summary>
    public int? IgnoreChunkIfLessThanThisAmountOfChars { get; init; } = 25;

    //todo support content format builder
}
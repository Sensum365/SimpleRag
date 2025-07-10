using SimpleRag.DataSources.Models;

namespace SimpleRag.DataSources.Markdown.Models;

/// <summary>
/// Base class for markdown sources.
/// </summary>
public abstract class MarkdownSource : DataSource
{
    /// <summary>Gets or sets a value indicating whether HTML comments should be ignored.</summary>
    public bool IgnoreCommentedOutContent { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether image references should be ignored.</summary>
    public bool IgnoreImages { get; set; } = true;

    /// <summary>Gets or sets the line count threshold for chunking files.</summary>
    public int? OnlyChunkIfMoreThanThisNumberOfLines { get; set; } = 25;

    /// <summary>Gets or sets the heading levels to chunk at.</summary>
    public int LevelsToChunk { get; set; } = 2;

    /// <summary>Gets or sets patterns for lines to ignore when chunking.</summary>
    public string? ChunkLineIgnorePatterns { get; set; }

    /// <summary>Gets or sets the minimum size of a chunk in characters.</summary>
    public int? IgnoreChunkIfLessThanThisAmountOfChars { get; set; } = 25;
}
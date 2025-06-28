using SimpleRag.DataSources.Models;

namespace SimpleRag.DataSources.Markdown.Models;

public abstract class MarkdownSource : DataSource
{
    public bool IgnoreCommentedOutContent { get; set; } = true;
    public bool IgnoreImages { get; set; } = true;
    public int? OnlyChunkIfMoreThanThisNumberOfLines { get; set; } = 25;
    public int LevelsToChunk { get; set; } = 2;
    public string? ChunkLineIgnorePatterns { get; set; }
    public int? IgnoreChunkIfLessThanThisAmountOfChars { get; set; } = 25;
}
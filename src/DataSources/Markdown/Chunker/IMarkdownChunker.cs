namespace SimpleRag.DataSources.Markdown.Chunker;

/// <summary>
/// Represent a MarkdownChunker
/// </summary>
public interface IMarkdownChunker
{
    /// <summary>
    /// Breaks the specified markdown content into chunks.
    /// </summary>
    /// <param name="content">The markdown content.</param>
    /// <param name="level">The heading level to chunk at.</param>
    /// <param name="linesToIgnorePatterns">Optional patterns of lines to ignore.</param>
    /// <param name="ignoreIfLessThanThisAmountOfChars">Optional minimum size for a chunk.</param>
    /// <returns>The created chunks.</returns>
    MarkdownChunk[] GetChunks(string content, int level = 3, string? linesToIgnorePatterns = null, int? ignoreIfLessThanThisAmountOfChars = null);
}
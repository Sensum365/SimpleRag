namespace SimpleRag.DataSources.Markdown.Chunker;

/// <summary>
/// Represents a chunk of markdown content.
/// </summary>
/// <param name="chunkId">Identifier of the chunk.</param>
/// <param name="name">The display name of the chunk.</param>
/// <param name="content">The chunk content.</param>
public class MarkdownChunk(string chunkId, string name, string content)
{
    /// <summary>
    /// Identifier of the chunk.
    /// </summary>
    public string ChunkId { get; } = chunkId;

    /// <summary>
    /// The display name of the chunk.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The chunk content.
    /// </summary>
    public string Content { get; } = content;

    /// <summary>
    /// SourcePath ot the chunk
    /// </summary>
    public string SourcePath { get; set; } = "";
}
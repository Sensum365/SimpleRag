namespace SimpleRag.DataSources.Markdown.Chunker;

/// <summary>
/// Represents a chunk of markdown content.
/// </summary>
/// <param name="ChunkId">Identifier of the chunk.</param>
/// <param name="Name">The display name of the chunk.</param>
/// <param name="Content">The chunk content.</param>
public record MarkdownChunk(string ChunkId, string Name, string Content);
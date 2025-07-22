using JetBrains.Annotations;
using SimpleRag.DataProviders.Models;

namespace SimpleRag.DataSources.PowerPoint.Chunker;

/// <summary>
/// Represent a PowerPoint chunker that can break down a presentation file into smaller chunks
/// </summary>
[PublicAPI]
public interface IPowerPointChunker
{
    /// <summary>
    /// Get the chunks of a PowerPoint file
    /// </summary>
    /// <param name="file">The file to chunk</param>
    /// <returns>The created chunks</returns>
    PowerPointChunk[] GetChunks(FileContent file);
}

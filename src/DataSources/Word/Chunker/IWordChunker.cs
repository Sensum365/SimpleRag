using JetBrains.Annotations;
using SimpleRag.DataProviders.Models;

namespace SimpleRag.DataSources.Word.Chunker;

/// <summary>
/// Represent a Word chunker that can break down a docx file into smaller chunks
/// </summary>
[PublicAPI]
public interface IWordChunker
{
    /// <summary>
    /// Get the chunks of a Word file
    /// </summary>
    /// <param name="file">The file to chunk</param>
    /// <returns>The created chunks</returns>
    WordChunk[] GetChunks(FileContent file);
}

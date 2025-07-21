using JetBrains.Annotations;
using SimpleRag.DataProviders.Models;

namespace SimpleRag.DataSources.Pdf.Chunker;

/// <summary>
/// Represent an PDF Chunker that can break down a PDF File into smaller chunks
/// </summary>
[PublicAPI]
public interface IPdfChunker
{
    /// <summary>
    /// Get the Chunks of a PDF
    /// </summary>
    /// <param name="file">The PDF File to Chunk</param>
    /// <returns>Chunks</returns>
    PdfChunk[] GetChunks(FileContent file);
}
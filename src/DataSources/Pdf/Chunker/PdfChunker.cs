using SimpleRag.DataProviders.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SimpleRag.DataSources.Pdf.Chunker;

/// <summary>
/// PDF Chunker that can break down a PDF File into smaller chunks
/// </summary>
public class PdfChunker : IPdfChunker
{
    /// <summary>
    /// Get the Chunks of a PDF
    /// </summary>
    /// <param name="file">The PDF File to Chunk</param>
    /// <returns>Chunks</returns>
    public PdfChunk[] GetChunks(FileContent file)
    {
        List<PdfChunk> chunks = [];
        PdfDocument document = PdfDocument.Open(file.Bytes);
        int pageNumber = 1;
        foreach (Page page in document.GetPages())
        {
            string pageText = page.Text;
            if (string.IsNullOrWhiteSpace(pageText))
            {
                continue;
            }

            string filename = Path.GetFileNameWithoutExtension(file.Path);
            string folder = file.PathWithoutRoot;
            pageNumber++;
            chunks.Add(new PdfChunk(filename, folder, pageNumber, document.NumberOfPages, pageText));
        }

        return chunks.ToArray();
    }
}
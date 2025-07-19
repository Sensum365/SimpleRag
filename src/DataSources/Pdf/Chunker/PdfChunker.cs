using SimpleRag.DataProviders.Models;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SimpleRag.DataSources.Pdf.Chunker;

public class PdfChunker : IPdfChunker
{
    public PdfChunk[] GetChunks(FileContent file)
    {
        List<PdfChunk> chunks = new List<PdfChunk>();
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
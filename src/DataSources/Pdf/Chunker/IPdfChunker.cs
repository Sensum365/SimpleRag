using SimpleRag.DataProviders.Models;
using System.Diagnostics.Metrics;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SimpleRag.DataSources.Pdf.Chunker;

public interface IPdfChunker
{
    PdfChunk[] GetChunks(FileContent file);
}
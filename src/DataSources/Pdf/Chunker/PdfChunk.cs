namespace SimpleRag.DataSources.Pdf.Chunker;

/// <summary>
/// Represent a chunk of a PDF
/// </summary>
/// <param name="name">Name of the PDF</param>
/// <param name="page">Page of the PDF</param>
/// <param name="totalPages">Total Pages in the PDF</param>
/// <param name="text">Raw Text of the PDF</param>
public class PdfChunk(string name, int page, int totalPages, string text)
{
    /// <summary>Name of the PDF</summary>
    public string Name { get; } = name;

    /// <summary>Page of the PDF</summary>
    public int Page { get; } = page;

    /// <summary>Total Pages in the PDF</summary>
    public int TotalPages { get; } = totalPages;

    /// <summary>Raw Text of the PDF</summary>
    public string Text { get; } = text;

    /// <summary>
    /// SourcePath ot the chunk
    /// </summary>
    public string SourcePath { get; set; } = "";
}
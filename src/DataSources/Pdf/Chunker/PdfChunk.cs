namespace SimpleRag.DataSources.Pdf.Chunker;

/// <summary>
/// Represent a chunk of a PDF
/// </summary>
/// <param name="Filename">Filename of the PDF</param>
/// <param name="Folder">Folder of the PDF</param>
/// <param name="Page">Page of the PDF</param>
/// <param name="TotalPages">Total Pages in the PDF</param>
/// <param name="Text">Raw Text of the PDF</param>
public record PdfChunk(string Filename, string Folder, int Page, int TotalPages, string Text);
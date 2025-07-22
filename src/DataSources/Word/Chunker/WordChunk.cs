using JetBrains.Annotations;

namespace SimpleRag.DataSources.Word.Chunker;

/// <summary>
/// Represent a chunk of a Word document
/// </summary>
/// <param name="name">Name of the document</param>
/// <param name="paragraph">Paragraph number</param>
/// <param name="totalParagraphs">Total paragraphs</param>
/// <param name="text">Raw text content</param>
[PublicAPI]
public class WordChunk(string name, int paragraph, int totalParagraphs, string text)
{
    /// <summary>Name of the document</summary>
    public string Name { get; } = name;

    /// <summary>Paragraph number</summary>
    public int Paragraph { get; } = paragraph;

    /// <summary>Total paragraphs in the document</summary>
    public int TotalParagraphs { get; } = totalParagraphs;

    /// <summary>Raw text content of the paragraph</summary>
    public string Text { get; } = text;

    /// <summary>Source path of the chunk</summary>
    public string SourcePath { get; init; } = "";
}

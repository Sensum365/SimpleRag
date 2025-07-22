using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using JetBrains.Annotations;
using SimpleRag.DataProviders.Models;

namespace SimpleRag.DataSources.Word.Chunker;

/// <summary>
/// Chunker that extracts text from Word documents
/// </summary>
[PublicAPI]
public class WordChunker : IWordChunker
{
    /// <summary>
    /// Get chunks of a Word document
    /// </summary>
    /// <param name="file">The Word file</param>
    /// <returns>Chunks created from each paragraph</returns>
    public WordChunk[] GetChunks(FileContent file)
    {
        List<WordChunk> chunks = [];
        using MemoryStream ms = new(file.Bytes);
        using WordprocessingDocument document = WordprocessingDocument.Open(ms, false);

        Body? body = document.MainDocumentPart?.Document.Body;
        if (body == null)
        {
            return [];
        }

        List<Paragraph> paragraphs = body.Descendants<Paragraph>().ToList();
        if (!paragraphs.Any())
        {
            return [];
        }

        int totalParagraphs = paragraphs.Count;
        int paragraphNumber = 1;

        foreach (Paragraph paragraph in paragraphs)
        {
            string text = string.Join(" ", paragraph.Descendants<Text>().Select(t => t.Text));
            if (string.IsNullOrWhiteSpace(text))
            {
                paragraphNumber++;
                continue;
            }

            string name = Path.GetFileNameWithoutExtension(file.Path);
            chunks.Add(new WordChunk(name, paragraphNumber, totalParagraphs, text)
            {
                SourcePath = file.PathWithoutRoot
            });
            paragraphNumber++;
        }

        return chunks.ToArray();
    }
}
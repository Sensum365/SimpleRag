using DocumentFormat.OpenXml.Packaging;
using JetBrains.Annotations;
using SimpleRag.DataProviders.Models;
using A = DocumentFormat.OpenXml.Drawing;

namespace SimpleRag.DataSources.PowerPoint.Chunker;

/// <summary>
/// Chunker that extracts text from PowerPoint presentations
/// </summary>
[PublicAPI]
public class PowerPointChunker : IPowerPointChunker
{
    /// <summary>
    /// Get chunks of a PowerPoint presentation
    /// </summary>
    /// <param name="file">The PowerPoint file</param>
    /// <returns>Chunks created from each slide</returns>
    public PowerPointChunk[] GetChunks(FileContent file)
    {
        List<PowerPointChunk> chunks = [];
        using MemoryStream ms = new(file.Bytes);
        using PresentationDocument presentation = PresentationDocument.Open(ms, false);
        var slides = presentation.PresentationPart?.SlideParts?.ToList();
        if (slides == null)
        {
            return [];
        }

        int totalSlides = slides.Count;
        int slideNumber = 1;
        foreach (var slidePart in slides)
        {
            string text = string.Join(" ", slidePart.Slide.Descendants<A.Text>().Select(t => t.Text));
            if (string.IsNullOrWhiteSpace(text))
            {
                slideNumber++;
                continue;
            }

            string name = Path.GetFileNameWithoutExtension(file.Path);
            chunks.Add(new PowerPointChunk(name, slideNumber, totalSlides, text)
            {
                SourcePath = file.PathWithoutRoot
            });
            slideNumber++;
        }

        return chunks.ToArray();
    }
}

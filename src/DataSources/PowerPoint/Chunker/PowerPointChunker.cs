using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using JetBrains.Annotations;
using SimpleRag.DataProviders.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using A = DocumentFormat.OpenXml.Drawing;

namespace SimpleRag.DataSources.PowerPoint.Chunker;

/// <summary>
/// Chunker that extracts text from PowerPoint presentations
/// </summary>
[PublicAPI]
[Experimental("RAG002")]
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

        var presentationPart = presentation.PresentationPart;
        if (presentationPart == null)
        {
            return [];
        }

        // Get all slide parts from the presentation
        var slideParts = presentationPart.SlideParts.ToList();
        if (!slideParts.Any())
        {
            return [];
        }

        // Create dictionary mapping between slide ID and slide part
        var slidePartMap = new Dictionary<string, SlidePart>();
        foreach (var slidePart in slideParts)
        {
            // Get relationship ID of slide part
            string relId = presentationPart.GetIdOfPart(slidePart);
            slidePartMap.Add(relId, slidePart);
        }

        // Get ordered slide IDs from presentation
        var slideIds = presentationPart.Presentation.SlideIdList?.ChildElements
            .OfType<SlideId>()
            .ToList();

        if (slideIds == null || !slideIds.Any())
        {
            return [];
        }

        // Process slides in the correct order
        int totalSlides = slideIds.Count;
        int slideNumber = 1;

        foreach (var slideId in slideIds)
        {
            string? relationshipId = slideId.RelationshipId?.Value;
            if (string.IsNullOrEmpty(relationshipId) || !slidePartMap.TryGetValue(relationshipId, out var slidePart))
            {
                slideNumber++;
                continue;
            }

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
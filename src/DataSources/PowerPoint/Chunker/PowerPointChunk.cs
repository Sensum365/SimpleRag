using JetBrains.Annotations;

namespace SimpleRag.DataSources.PowerPoint.Chunker;

/// <summary>
/// Represent a chunk of a PowerPoint presentation
/// </summary>
/// <param name="name">Name of the presentation</param>
/// <param name="slide">Slide number</param>
/// <param name="totalSlides">Total slides</param>
/// <param name="text">Raw text content</param>
[PublicAPI]
public class PowerPointChunk(string name, int slide, int totalSlides, string text)
{
    /// <summary>Name of the presentation</summary>
    public string Name { get; } = name;

    /// <summary>Slide number</summary>
    public int Slide { get; } = slide;

    /// <summary>Total slides in the presentation</summary>
    public int TotalSlides { get; } = totalSlides;

    /// <summary>Raw text content of the slide</summary>
    public string Text { get; } = text;

    /// <summary>Source path of the chunk</summary>
    public string SourcePath { get; init; } = "";
}

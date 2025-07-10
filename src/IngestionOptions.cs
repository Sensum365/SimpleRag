using JetBrains.Annotations;
using SimpleRag.DataSources.CSharp.Models;

namespace SimpleRag;

/// <summary>
/// Options used during ingestion of data sources.
/// </summary>
[PublicAPI]
public class IngestionOptions
{
    /// <summary>Gets or sets an optional function to build content for C# sources.</summary>
    public Func<CSharpChunk, string>? CSharpContentFormatBuilder { get; set; }
}
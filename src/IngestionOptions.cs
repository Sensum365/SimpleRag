using SimpleRag.DataSources.CSharp.Models;

namespace SimpleRag;

public class IngestionOptions
{
    public Func<CSharpChunk, string>? CSharpContentFormatBuilder { get; set; }
}
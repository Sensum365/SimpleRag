namespace SimpleRag.DataSources.CSharp.Chunker;

/// <summary>
/// Options for the C# Chunking
/// </summary>
public class CSharpChunkerOptions
{
    /// <summary>
    /// Indicates if Private and Internal members should be included (Default: False)
    /// </summary>
    public bool IncludeInternalAndPrivateMembers { get; set; }

    /// <summary>
    /// Indicates if Member Bodies (Methods, Constructors) should be included (Default: False)
    /// </summary>
    public bool IncludeMemberBodies { get; set; }
}
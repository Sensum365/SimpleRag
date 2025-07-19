namespace SimpleRag.DataSources.CSharp.Chunker;

/// <summary>
/// Representation of a C# Chunker
/// </summary>
public interface ICSharpChunker
{
    /// <summary>
    /// Parses the provided code and returns the discovered code entities.
    /// </summary>
    /// <param name="code">The code to analyze.</param>
    /// <returns>A list of discovered code chunks.</returns>
    List<CSharpChunk> GetChunks(string code);
}
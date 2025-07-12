using SimpleRag.DataSources.Models;

namespace SimpleRag.DataSources.CSharp.Models;

/// <summary>
/// Represent a C# Based Datasource
/// </summary>
public abstract class CSharpDataSource : DataSource
{
    /// <summary>
    /// Builder of the desired format of the Content to be vectorized or leave null to use the default provided format
    /// </summary>
    public Func<CSharpChunk, string>? CSharpContentFormatBuilder { get; set; }
}
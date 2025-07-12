using SimpleRag.FileContent.Models;

namespace SimpleRag.DataSources.CSharp.Models;

/// <summary>
/// Represents a local C# data source on disk.
/// </summary>
public class CSharpDataSourceLocal : CSharpDataSource
{
    /// <summary>
    /// Converts this instance to a <see cref="FileContentSourceLocal"/>.
    /// </summary>
    /// <returns>The created file content source.</returns>
    public FileContentSourceLocal AsFileContentSource()
    {
        return new FileContentSourceLocal
        {
            FileIgnorePatterns = FileIgnorePatterns,
            Path = Path,
            Recursive = Recursive
        };
    }
}
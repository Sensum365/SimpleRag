using SimpleRag.FileContent.Models;

namespace SimpleRag.DataSources.Markdown.Models;

/// <summary>
/// Represents a markdown source located on the local file system.
/// </summary>
public class MarkdownDataSourceLocal : MarkdownSource
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
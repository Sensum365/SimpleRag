using SimpleRag.FileContent.Models;

namespace SimpleRag.DataSources.Markdown.Models;

public class MarkdownDataSourceLocal : MarkdownSource
{
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
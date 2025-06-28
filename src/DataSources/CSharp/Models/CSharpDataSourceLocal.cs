using SimpleRag.DataSources.Models;
using SimpleRag.FileContent.Models;

namespace SimpleRag.DataSources.CSharp.Models;

public class CSharpDataSourceLocal : DataSource
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
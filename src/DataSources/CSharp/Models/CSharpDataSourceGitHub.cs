using SimpleRag.DataSources.Models;
using SimpleRag.FileContent.Models;

namespace SimpleRag.DataSources.CSharp.Models;

public class CSharpDataSourceGitHub : DataSource
{
    public required string GitHubOwner { get; set; }
    public required string GitHubRepo { get; set; }
    public DateTimeOffset? GitHubLastCommitTimestamp { get; set; }

    public FileContentSourceGitHub AsFileContentSource()
    {
        return new FileContentSourceGitHub
        {
            FileIgnorePatterns = FileIgnorePatterns,
            GitHubLastCommitTimestamp = GitHubLastCommitTimestamp,
            GitHubOwner = GitHubOwner,
            GitHubRepo = GitHubRepo,
            Path = Path,
            Recursive = Recursive
        };
    }
}
using SimpleRag.FileContent.Models;
using SimpleRag.Integrations.GitHub;

namespace SimpleRag.DataSources.Markdown.Models;

/// <summary>
/// Represents a markdown source retrieved from GitHub.
/// </summary>
public class MarkdownDataSourceGitHub : MarkdownDataSource
{
    /// <summary>
    /// Information about the GitHubRepo
    /// </summary>
    public required GitHubRepository GitHubRepository { get; init; }


    /// <summary>
    /// Converts this instance to a <see cref="FileContentSourceGitHub"/>.
    /// </summary>
    /// <returns>The created file content source.</returns>
    public FileContentSourceGitHub AsFileContentSource()
    {
        return new FileContentSourceGitHub
        {
            FileIgnorePatterns = FileIgnorePatterns,
            GitHubRepository = GitHubRepository,
            Path = Path,
            Recursive = Recursive
        };
    }
}
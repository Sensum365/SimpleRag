using SimpleRag.FileContent.Models;

namespace SimpleRag.DataSources.Markdown.Models;

/// <summary>
/// Represents a markdown source retrieved from GitHub.
/// </summary>
public class MarkdownDataSourceGitHub : MarkdownSource
{
    /// <summary>Gets or sets the GitHub repository owner.</summary>
    public required string GitHubOwner { get; set; }

    /// <summary>Gets or sets the GitHub repository name.</summary>
    public required string GitHubRepo { get; set; }

    /// <summary>Gets or sets the last commit timestamp processed.</summary>
    public DateTimeOffset? GitHubLastCommitTimestamp { get; set; }

    /// <summary>
    /// Converts this instance to a <see cref="FileContentSourceGitHub"/>.
    /// </summary>
    /// <returns>The created file content source.</returns>
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
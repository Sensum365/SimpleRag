using SimpleRag.DataSources.Models;
using SimpleRag.FileContent.Models;

namespace SimpleRag.DataSources.CSharp.Models;

/// <summary>
/// A C# Based Source that should be retrieved from GitHub
/// </summary>
public class CSharpDataSourceGitHub : DataSource
{
    /// <summary>
    /// Gets or sets the owner of the repository on GitHub.
    /// </summary>
    public required string GitHubOwner { get; set; }

    /// <summary>
    /// Gets or sets the name of the repository on GitHub.
    /// </summary>
    public required string GitHubRepo { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last commit to check for changes.
    /// </summary>
    public DateTimeOffset? GitHubLastCommitTimestamp { get; set; }

    /// <summary>
    /// Converts this object to a <see cref="FileContentSourceGitHub"/> object.
    /// </summary>
    /// <returns>A new <see cref="FileContentSourceGitHub"/> object.</returns>
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
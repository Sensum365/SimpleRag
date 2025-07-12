using SimpleRag.FileContent.Models;
using SimpleRag.Integrations.GitHub;

namespace SimpleRag.DataSources.CSharp.Models;

/// <summary>
/// A C# Based Source that should be retrieved from GitHub
/// </summary>
public class CSharpDataSourceGitHub : CSharpDataSource
{
    /// <summary>
    /// Information about the GitHubRepo
    /// </summary>
    public required GitHubRepository GitHubRepository { get; init; }

    /// <summary>
    /// Converts this object to a <see cref="FileContentSourceGitHub"/> object.
    /// </summary>
    /// <returns>A new <see cref="FileContentSourceGitHub"/> object.</returns>
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
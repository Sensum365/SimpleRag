using SimpleRag.Integrations.GitHub;

namespace SimpleRag.FileContent.Models;

/// <summary>
/// Describes a GitHub source for retrieving files.
/// </summary>
public class FileContentSourceGitHub : FileContentSource
{
    /// <summary>
    /// Information about the GitHubRepo
    /// </summary>
    public required GitHubRepository GitHubRepository { get; init; }
}
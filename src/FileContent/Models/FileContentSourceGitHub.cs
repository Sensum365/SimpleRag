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

    /// <summary>
    /// Gets or sets the timestamp of the last commit processed.
    /// </summary>
    public required DateTimeOffset? LastCommitTimestamp { get; init; }
}
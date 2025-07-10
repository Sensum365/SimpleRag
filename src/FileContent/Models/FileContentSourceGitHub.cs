namespace SimpleRag.FileContent.Models;

/// <summary>
/// Describes a GitHub source for retrieving files.
/// </summary>
public class FileContentSourceGitHub : FileContentSource
{
    /// <summary>Gets or sets the repository owner.</summary>
    public required string? GitHubOwner { get; set; }

    /// <summary>Gets or sets the repository name.</summary>
    public required string? GitHubRepo { get; set; }

    /// <summary>Gets or sets the timestamp of the last commit processed.</summary>
    public required DateTimeOffset? GitHubLastCommitTimestamp { get; set; }
}

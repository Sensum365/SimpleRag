namespace SimpleRag.FileContent.Models;

public class FileContentSourceGitHub : FileContentSource
{
    public required string? GitHubOwner { get; set; }
    public required string? GitHubRepo { get; set; }
    public required DateTimeOffset? GitHubLastCommitTimestamp { get; set; }
}
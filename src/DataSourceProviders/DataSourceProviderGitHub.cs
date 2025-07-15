using SimpleRag.Integrations.GitHub;

namespace SimpleRag.DataSourceProviders;

/// <summary>
/// A SourceProvider representing files in a GitHub Repository
/// </summary>
public class DataSourceProviderGitHub : IDataSourceProvider
{
    /// <summary>
    /// Information about the GitHubRepo
    /// </summary>
    public required GitHubRepository GitHubRepository { get; init; }

    /// <summary>
    /// Gets or sets the timestamp of the last commit processed.
    /// </summary>
    public DateTimeOffset? LastCommitTimestamp { get; init; }
}
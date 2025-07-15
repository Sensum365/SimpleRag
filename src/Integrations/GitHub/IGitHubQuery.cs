using Octokit;

namespace SimpleRag.Integrations.GitHub;

public interface IGitHubQuery
{
    /// <summary>Gets a value indicating whether a GitHub token is configured.</summary>
    bool IsGitHubTokenProvided { get; }

    /// <summary>
    /// Creates a new authenticated GitHub client.
    /// </summary>
    /// <returns>The GitHub client.</returns>
    GitHubClient GetGitHubClient();

    /// <summary>
    /// Gets the tree for the specified commit.
    /// </summary>
    Task<TreeResponse> GetTreeAsync(GitHubClient client, Commit commit, GitHubRepository repo, bool recursive);

    /// <summary>
    /// Gets the latest commit of the specified repository.
    /// </summary>
    Task<Commit> GetLatestCommitAsync(GitHubClient client, GitHubRepository repo);

    /// <summary>
    /// Retrieves the raw file content from GitHub.
    /// </summary>
    Task<string?> GetFileContentAsync(GitHubClient client, GitHubRepository repo, string path);
}
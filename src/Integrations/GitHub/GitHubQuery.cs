using System.Text;
using JetBrains.Annotations;
using Octokit;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace SimpleRag.Integrations.GitHub;

/// <summary>
/// Helper methods for interacting with the GitHub API.
/// </summary>
[PublicAPI]
public class GitHubQuery(GitHubConnection connection)
{
    /// <summary>Gets a value indicating whether a GitHub token is configured.</summary>
    public bool IsGitHubTokenProvided => !string.IsNullOrWhiteSpace(connection.GitHubToken);

    /// <summary>
    /// Creates a new authenticated GitHub client.
    /// </summary>
    /// <returns>The GitHub client.</returns>
    public GitHubClient GetGitHubClient()
    {
        if (string.IsNullOrWhiteSpace(connection.GitHubToken))
        {
            throw new GitHubIntegrationException("The optional GitHubToken configuration variable is not set so can't interact with GitHubApi");
        }

        return new GitHubClient(new ProductHeaderValue("CodeRag"))
        {
            Credentials = new Credentials(connection.GitHubToken)
        };
    }

    /// <summary>
    /// Gets the tree for the specified commit.
    /// </summary>
    public async Task<TreeResponse> GetTreeAsync(GitHubClient client, Commit commit, GitHubRepository repo, bool recursive)
    {
        if (recursive)
        {
            return await client.Git.Tree.GetRecursive(repo.Owner, repo.Name, commit.Tree.Sha);
        }

        return await client.Git.Tree.Get(repo.Owner, repo.Name, commit.Tree.Sha);
    }

    /// <summary>
    /// Gets the latest commit of the specified repository.
    /// </summary>
    public async Task<Commit> GetLatestCommitAsync(GitHubClient client, GitHubRepository repo)
    {
        Repository repository = await client.Repository.Get(repo.Owner, repo.Name);
        string defaultBranch = repository.DefaultBranch; //todo - support other branches (https://github.com/rwjdk/CodeRag/issues/2)

        Reference reference = await client.Git.Reference.Get(repo.Owner, repo.Name, $"heads/{defaultBranch}");

        return await client.Git.Commit.Get(repo.Owner, repo.Name, reference.Object.Sha);
    }

    /// <summary>
    /// Retrieves the raw file content from GitHub.
    /// </summary>
    public async Task<string?> GetFileContentAsync(GitHubClient client, GitHubRepository repo, string path)
    {
        byte[]? fileContent = await client.Repository.Content.GetRawContent(repo.Owner, repo.Name, path);
        return fileContent == null ? null : Encoding.UTF8.GetString(fileContent);
    }
}
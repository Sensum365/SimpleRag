namespace SimpleRag.Integrations.GitHub.Models;

/// <summary>
/// Configuration for connecting to GitHub.
/// </summary>
/// <param name="GitHubToken">The personal access token.</param>
public record GitHubCredentials(string GitHubToken);
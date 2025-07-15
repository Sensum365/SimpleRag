namespace SimpleRag.Integrations.GitHub.Models;

/// <summary>
/// Represent a GitHub Repository
/// </summary>
public class GitHubRepository
{
    /// <summary>
    /// Gets or sets the repository owner.
    /// </summary>
    public required string? Owner { get; init; }

    /// <summary>
    /// Gets or sets the repository name.
    /// </summary>
    public required string? Name { get; init; }
}
using JetBrains.Annotations;

namespace SimpleRag.Integrations.GitHub.Models;

/// <summary>
/// Represent a GitHub Repository
/// </summary>
[PublicAPI]
public record GitHubRepository(string Owner, string Name);
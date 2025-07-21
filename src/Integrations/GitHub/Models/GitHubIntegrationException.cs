using JetBrains.Annotations;

namespace SimpleRag.Integrations.GitHub.Models;

/// <summary>
/// Exception thrown when GitHub integration fails.
/// </summary>
[PublicAPI]
public class GitHubIntegrationException(string message, Exception? innerException = null) : Exception(message, innerException);
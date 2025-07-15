namespace SimpleRag.Integrations.GitHub.Models;

/// <summary>
/// Exception thrown when GitHub integration fails.
/// </summary>
public class GitHubIntegrationException(string message, Exception? innerException = null) : Exception(message, innerException);
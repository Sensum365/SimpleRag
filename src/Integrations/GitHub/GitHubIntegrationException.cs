namespace SimpleRag.Integrations.GitHub;

public class GitHubIntegrationException(string message, Exception? innerException = null) : Exception(message, innerException);
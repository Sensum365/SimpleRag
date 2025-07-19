using JetBrains.Annotations;

namespace SimpleRag.Integrations.GitHub.Models;

/// <summary>
/// Configuration for connecting to GitHub.
/// </summary>
[PublicAPI]
public record GitHubCredentials
{
    /// <summary>
    /// AppId (Credentials using GitHubApp)
    /// </summary>
    public string? AppId { get; }

    /// <summary>
    /// Private Key (Credentials using GitHubApp)
    /// </summary>
    public string? PrivateKey { get; }

    /// <summary>
    /// Token (Credentials using PAT)
    /// </summary>
    public string? PersonalAccessToken { get; }

    /// <summary>
    /// Constructor (PAT)
    /// </summary>
    /// <param name="personalAccessToken">Personal Access Token</param>
    public GitHubCredentials(string personalAccessToken)
    {
        PersonalAccessToken = personalAccessToken;
    }

    /// <summary>
    /// Constructor (GitHubApp)
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="privateKey"></param>
    public GitHubCredentials(string appId, string privateKey)
    {
        AppId = appId;
        PrivateKey = privateKey;
    }
}
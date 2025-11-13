using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Octokit;
using SimpleRag.Integrations.GitHub.Models;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace SimpleRag.Integrations.GitHub;

internal class GitHubQuery(GitHubCredentials credentials)
{
    public async Task<GitHubClient> GetGitHubClientAsync()
    {
        if (!string.IsNullOrWhiteSpace(credentials.PersonalAccessToken))
        {
            return new GitHubClient(new ProductHeaderValue("SimpleRag"))
            {
                Credentials = new Credentials(credentials.PersonalAccessToken)
            };
        }

        if (!string.IsNullOrWhiteSpace(credentials.AppId) && !string.IsNullOrWhiteSpace(credentials.PrivateKey))
        {
            string jwtToken = GenerateJwtToken(credentials.PrivateKey, credentials.AppId);
            GitHubClient githubClient = new GitHubClient(new ProductHeaderValue("SimpleRag"))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            IReadOnlyList<Installation>? installations = await githubClient.GitHubApps.GetAllInstallationsForCurrent();

            // Get installation token for the first installation
            long installationId = installations[0].Id;
            AccessToken? installationToken = await githubClient.GitHubApps.CreateInstallationToken(installationId);

            githubClient.Credentials = new Credentials(installationToken.Token);
            return githubClient;
        }

        throw new GitHubIntegrationException("No valid GitHub Credentials where given");

        string GenerateJwtToken(string privateKey, string appId)
        {
            RSA rsa = RSA.Create();
            rsa.ImportFromPem(privateKey);

            try
            {
                RSAParameters rsaParameters = rsa.ExportParameters(true); // Export key material
                DateTimeOffset now = DateTimeOffset.UtcNow;

                // Use the key material to create a new independent RSA key
                RSA independentRsa = RSA.Create();
                independentRsa.ImportParameters(rsaParameters);

                // Create the signing key and credentials
                RsaSecurityKey rsaSecurityKey = new RsaSecurityKey(independentRsa);
                SigningCredentials signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

                // Create the JWT Header and Payload
                JwtSecurityToken securityToken = new JwtSecurityToken(
                    issuer: appId,
                    claims: null,
                    notBefore: now.UtcDateTime,
                    expires: now.AddMinutes(9).UtcDateTime,
                    signingCredentials: signingCredentials
                )
                {
                    Payload =
                    {
                        ["iat"] = now.ToUnixTimeSeconds()
                    }
                };

                return new JwtSecurityTokenHandler().WriteToken(securityToken);
            }
            finally
            {
                rsa.Dispose(); // Dispose of the original RSA instance
            }
        }
    }

    public async Task<TreeResponse> GetTreeAsync(GitHubClient client, Commit commit, GitHubRepository repo, bool recursive)
    {
        if (recursive)
        {
            return await client.Git.Tree.GetRecursive(repo.Owner, repo.Name, commit.Tree.Sha);
        }

        return await client.Git.Tree.Get(repo.Owner, repo.Name, commit.Tree.Sha);
    }

    public async Task<Commit> GetLatestCommitAsync(GitHubClient client, GitHubRepository repo)
    {
        Repository repository = await client.Repository.Get(repo.Owner, repo.Name);
        string defaultBranch = repository.DefaultBranch; //todo - support other branches (https://github.com/rwjdk/CodeRag/issues/2)

        Reference reference = await client.Git.Reference.Get(repo.Owner, repo.Name, $"heads/{defaultBranch}");

        return await client.Git.Commit.Get(repo.Owner, repo.Name, reference.Object.Sha);
    }

    public async Task<byte[]?> GetFileContentAsync(GitHubClient client, GitHubRepository repo, string path)
    {
        byte[]? fileContent = await client.Repository.Content.GetRawContent(repo.Owner, repo.Name, path);
        return fileContent;
    }
}
using Octokit;
using SimpleRag.FileContent;
using SimpleRag.FileContent.Models;
using SimpleRag.Integrations.GitHub;
using SimpleRag.Models;

namespace SimpleRag.DataSourceProviders;

/// <summary>
/// A SourceProvider representing files in a GitHub Repository
/// </summary>
public class DataSourceProviderGitHub(GitHubQuery gitHubQuery) : IDataSourceProvider
{
    /// <summary>
    /// Information about the GitHubRepo
    /// </summary>
    public required GitHubRepository GitHubRepository { get; init; }

    /// <summary>
    /// Gets or sets the timestamp of the last commit processed.
    /// </summary>
    public DateTimeOffset? LastCommitTimestamp { get; init; }

    public async Task<FileContent.Models.FileContent[]?> GetFileContent(FileContentSource source, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(source.Path))
        {
            throw new DataSourceProviderException("Path is not defined");
        }

        if (GitHubRepository == null || string.IsNullOrWhiteSpace(GitHubRepository.Owner) || string.IsNullOrWhiteSpace(GitHubRepository.Name))
        {
            throw new DataSourceProviderException("GitHub Owner and Repo is not defined");
        }

        List<FileContent.Models.FileContent> result = [];

        onProgressNotification?.Invoke(ProgressNotification.Create("Exploring GitHub"));
        var gitHubClient = gitHubQuery.GetGitHubClient();

        var commit = await gitHubQuery.GetLatestCommitAsync(gitHubClient, GitHubRepository);
        cancellationToken.ThrowIfCancellationRequested();
        if (LastCommitTimestamp.HasValue && commit.Committer.Date <= LastCommitTimestamp.Value)
        {
            onProgressNotification?.Invoke(ProgressNotification.Create("No new Commits detected in the repo so skipping retrieval"));
            return null;
        }

        var treeResponse = await gitHubQuery.GetTreeAsync(gitHubClient, commit, GitHubRepository, source.Recursive);
        string fileExtensionType = "." + source.FileExtensionType;
        cancellationToken.ThrowIfCancellationRequested();

        TreeItem[] items;
        if (source.Path == "/")
        {
            //Root defined
            items = treeResponse.Tree.Where(x => x.Type == TreeType.Blob && x.Path.EndsWith(fileExtensionType, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        }
        else
        {
            string prefix = source.Path;
            if (!prefix.EndsWith("/"))
            {
                prefix += "/";
            }

            items = treeResponse.Tree.Where(x => x.Type == TreeType.Blob && x.Path.StartsWith(prefix) && x.Path.EndsWith(fileExtensionType, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        }

        onProgressNotification?.Invoke(ProgressNotification.Create($"Found {items.Length} files"));

        List<string> ignoredFiles = [];
        int counter = 0;
        foreach (string path in items.Select(x => x.Path))
        {
            var pathWithoutRoot = path.Replace(source.Path, string.Empty);
            counter++;
            if (source.IgnoreFile(path))
            {
                onProgressNotification?.Invoke(ProgressNotification.Create("Ignoring file from GitHub", counter, items.Length, pathWithoutRoot));
                ignoredFiles.Add(path);
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();

            onProgressNotification?.Invoke(ProgressNotification.Create("Downloading file-content from GitHub", counter, items.Length, pathWithoutRoot));
            var content = await gitHubQuery.GetFileContentAsync(gitHubClient, GitHubRepository, path);
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            result.Add(new FileContent.Models.FileContent(path, content, pathWithoutRoot));
        }

        if (ignoredFiles.Count > 0)
        {
            onProgressNotification?.Invoke(ProgressNotification.Create($"{ignoredFiles.Count} Files Ignored"));
        }

        return result.ToArray();
    }
}
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using SimpleRag.DataProviders.Models;
using SimpleRag.Integrations.GitHub;
using SimpleRag.Integrations.GitHub.Models;

namespace SimpleRag.DataProviders;

/// <summary>
/// A SourceProvider representing files in a GitHub Repository
/// </summary>
public class GitHubFilesDataProvider : IFileContentProvider
{
    private readonly IGitHubQuery _gitHubQuery;

    /// <summary>
    /// A SourceProvider representing files in a GitHub Repository
    /// </summary>
    public GitHubFilesDataProvider(IGitHubQuery gitHubQuery)
    {
        _gitHubQuery = gitHubQuery;
    }

    /// <summary>
    /// A SourceProvider representing files in a GitHub Repository
    /// </summary>
    public GitHubFilesDataProvider(IServiceProvider serviceProvider)
    {
        _gitHubQuery = serviceProvider.GetRequiredService<IGitHubQuery>();
    }

    /// <summary>
    /// Information about the GitHubRepo
    /// </summary>
    public required GitHubRepository GitHubRepository { get; init; }

    /// <summary>
    /// Gets or sets the timestamp of the last commit processed.
    /// </summary>
    public DateTimeOffset? LastCommitTimestamp { get; init; }

    /// <summary>
    /// Get FileContent of a GitHub Provider
    /// </summary>
    /// <param name="source">The FileContent Source</param>
    /// <param name="onProgressNotification">Action to notify progress</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task<FileContent[]?> GetFileContent(FileContentSource source, Action<Notification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(source.Path))
        {
            throw new DataProviderException("Path is not defined");
        }

        if (GitHubRepository == null || string.IsNullOrWhiteSpace(GitHubRepository.Owner) || string.IsNullOrWhiteSpace(GitHubRepository.Name))
        {
            throw new DataProviderException("GitHub Owner and Repo is not defined");
        }

        List<FileContent> result = [];

        onProgressNotification?.Invoke(Notification.Create("Exploring GitHub"));
        var gitHubClient = _gitHubQuery.GetGitHubClient();

        var commit = await _gitHubQuery.GetLatestCommitAsync(gitHubClient, GitHubRepository);
        cancellationToken.ThrowIfCancellationRequested();
        if (LastCommitTimestamp.HasValue && commit.Committer.Date <= LastCommitTimestamp.Value)
        {
            onProgressNotification?.Invoke(Notification.Create("No new Commits detected in the repo so skipping retrieval"));
            return null;
        }

        var treeResponse = await _gitHubQuery.GetTreeAsync(gitHubClient, commit, GitHubRepository, source.Recursive);
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

        onProgressNotification?.Invoke(Notification.Create($"Found {items.Length} files"));

        List<string> ignoredFiles = [];
        int counter = 0;
        foreach (string path in items.Select(x => x.Path))
        {
            var pathWithoutRoot = path.Replace(source.Path, string.Empty);
            counter++;
            if (source.IgnoreFile(path))
            {
                onProgressNotification?.Invoke(Notification.Create("Ignoring file from GitHub", counter, items.Length, pathWithoutRoot));
                ignoredFiles.Add(path);
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();

            onProgressNotification?.Invoke(Notification.Create("Downloading file-content from GitHub", counter, items.Length, pathWithoutRoot));
            var bytes = await _gitHubQuery.GetFileContentAsync(gitHubClient, GitHubRepository, path);
            if (bytes == null)
            {
                continue;
            }

            result.Add(new FileContent(path, bytes, pathWithoutRoot));
        }

        if (ignoredFiles.Count > 0)
        {
            onProgressNotification?.Invoke(Notification.Create($"{ignoredFiles.Count} Files Ignored"));
        }

        return result.ToArray();
    }
}
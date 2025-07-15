using JetBrains.Annotations;
using Octokit;
using SimpleRag.FileContent.Models;
using SimpleRag.Integrations.GitHub;
using SimpleRag.Models;
using System.Text;
using System.Text.RegularExpressions;
using SimpleRag.DataSourceProviders;

namespace SimpleRag.FileContent;

/// <summary>
/// Base class for querying file content sources.
/// </summary>
[PublicAPI]
public class FileContentQuery(GitHubQuery gitHubQuery)
{
    /// <summary>
    /// Guards for input
    /// </summary>
    /// <param name="source"></param>
    /// <exception cref="FileContentException"></exception>
    protected void SharedGuards(FileContentSource source)
    {
        if (string.IsNullOrWhiteSpace(source.Path))
        {
            throw new FileContentException("Path is not defined");
        }
    }

    /// <summary>
    /// Notify how many Number of files found
    /// </summary>
    /// <param name="numberOfFiles">Number of files</param>
    /// <param name="onProgressNotification">Notification</param>
    protected void NotifyNumberOfFilesFound(int numberOfFiles, Action<ProgressNotification>? onProgressNotification)
    {
        onProgressNotification?.Invoke(ProgressNotification.Create($"Found {numberOfFiles} files"));
    }

    /// <summary>
    /// Notify of what files where ignored
    /// </summary>
    /// <param name="ignoredFiles"></param>
    /// <param name="onProgressNotification">Notification</param>
    protected void NotifyIgnoredFiles(List<string> ignoredFiles, Action<ProgressNotification>? onProgressNotification)
    {
        if (ignoredFiles.Count > 0)
        {
            onProgressNotification?.Invoke(ProgressNotification.Create($"{ignoredFiles.Count} Files Ignored"));
        }
    }

    /// <summary>
    /// Determines whether the specified path should be ignored based on patterns.
    /// </summary>
    public bool IgnoreFile(string path, string fileIgnorePatterns)
    {
        if (string.IsNullOrWhiteSpace(fileIgnorePatterns))
        {
            return false;
        }

        string[] patternsToIgnore = fileIgnorePatterns.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (string pattern in patternsToIgnore.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            if (Regex.IsMatch(path, pattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Get Raw File content from a source
    /// </summary>
    /// <param name="source">The Datasource</param>
    /// <param name="fileExtensionType">The File Extension</param>
    /// <param name="onProgressNotification">Action on Progress Notification</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task<Models.FileContent[]?> GetRawContentForSourceAsync(FileContentSource source, string fileExtensionType, Action<ProgressNotification>? onProgressNotification, CancellationToken cancellationToken)
    {
        switch (source.Provider)
        {
            case DataSourceProviderGitHub gitHubSourceProvider:
                return await GetGitHubAsync(source, gitHubSourceProvider, fileExtensionType, onProgressNotification, cancellationToken);
            case DataSourceProviderLocal localSourceProvider:
                return await GetLocalAsync(source, localSourceProvider, fileExtensionType, onProgressNotification, cancellationToken);
            default:
                throw new ArgumentOutOfRangeException(nameof(source.Provider), "Unknown Provider");
        }
    }

    private async Task<Models.FileContent[]?> GetGitHubAsync(FileContentSource source, DataSourceProviderGitHub provider, string fileExtensionType, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        SharedGuards(source);

        if (provider.GitHubRepository == null || string.IsNullOrWhiteSpace(provider.GitHubRepository.Owner) || string.IsNullOrWhiteSpace(provider.GitHubRepository.Name))
        {
            throw new FileContentException("GitHub Owner and Repo is not defined");
        }

        List<Models.FileContent> result = [];

        onProgressNotification?.Invoke(ProgressNotification.Create("Exploring GitHub"));
        var gitHubClient = gitHubQuery.GetGitHubClient();

        var commit = await gitHubQuery.GetLatestCommitAsync(gitHubClient, provider.GitHubRepository);
        cancellationToken.ThrowIfCancellationRequested();
        if (provider.LastCommitTimestamp.HasValue && commit.Committer.Date <= provider.LastCommitTimestamp.Value)
        {
            onProgressNotification?.Invoke(ProgressNotification.Create("No new Commits detected in the repo so skipping retrieval"));
            return null;
        }

        var treeResponse = await gitHubQuery.GetTreeAsync(gitHubClient, commit, provider.GitHubRepository, source.Recursive);
        fileExtensionType = "." + fileExtensionType;
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

        NotifyNumberOfFilesFound(items.Length, onProgressNotification);
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
            var content = await gitHubQuery.GetFileContentAsync(gitHubClient, provider.GitHubRepository, path);
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            result.Add(new Models.FileContent(path, content, pathWithoutRoot));
        }

        NotifyIgnoredFiles(ignoredFiles, onProgressNotification);

        return result.ToArray();
    }

    private async Task<Models.FileContent[]?> GetLocalAsync(FileContentSource source, DataSourceProviderLocal provider, string fileExtensionType, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        SharedGuards(source);

        List<Models.FileContent> result = [];

        string[] files = Directory.GetFiles(source.Path, "*." + fileExtensionType, source.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        NotifyNumberOfFilesFound(files.Length, onProgressNotification);

        List<string> ignoredFiles = [];
        int counter = 0;
        foreach (string path in files)
        {
            if (source.IgnoreFile(path))
            {
                ignoredFiles.Add(path);
                continue;
            }

            counter++;
            onProgressNotification?.Invoke(ProgressNotification.Create("Parsing Local files from Disk", counter, files.Length));
            var pathWithoutRoot = path.Replace(source.Path, string.Empty);
            string content = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
            result.Add(new Models.FileContent(path, content, pathWithoutRoot));
        }

        NotifyIgnoredFiles(ignoredFiles, onProgressNotification);

        return result.ToArray();
    }
}
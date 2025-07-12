using JetBrains.Annotations;
using Octokit;
using SimpleRag.FileContent.Models;
using SimpleRag.Integrations.GitHub;
using SimpleRag.Models;

namespace SimpleRag.FileContent;

/// <summary>
/// Retrieves file content from GitHub sources.
/// </summary>
[PublicAPI]
public class FileContentGitHubQuery(GitHubQuery gitHubQuery) : FileContentQuery
{
    /// <summary>
    /// Gets the raw content for a GitHub source.
    /// </summary>
    public async Task<Models.FileContent[]?> GetRawContentForSourceAsync(FileContentSourceGitHub source, string fileExtensionType, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        SharedGuards(source);

        if (source.GitHubRepository == null || string.IsNullOrWhiteSpace(source.GitHubRepository.Owner) || string.IsNullOrWhiteSpace(source.GitHubRepository.Name))
        {
            throw new FileContentException("GitHub Owner and Repo is not defined");
        }

        List<Models.FileContent> result = [];

        onProgressNotification?.Invoke(ProgressNotification.Create("Exploring GitHub"));
        var gitHubClient = gitHubQuery.GetGitHubClient();

        var commit = await gitHubQuery.GetLatestCommitAsync(gitHubClient, source.GitHubRepository);
        cancellationToken.ThrowIfCancellationRequested();
        if (source.LastCommitTimestamp.HasValue && commit.Committer.Date <= source.LastCommitTimestamp.Value)
        {
            onProgressNotification?.Invoke(ProgressNotification.Create("No new Commits detected in the repo so skipping retrieval"));
            return null;
        }

        var treeResponse = await gitHubQuery.GetTreeAsync(gitHubClient, commit, source.GitHubRepository, source.Recursive);
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
            var content = await gitHubQuery.GetFileContentAsync(gitHubClient, source.GitHubRepository, path);
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            result.Add(new Models.FileContent(path, content, pathWithoutRoot));
        }

        NotifyIgnoredFiles(ignoredFiles, onProgressNotification);

        return result.ToArray();
    }
}
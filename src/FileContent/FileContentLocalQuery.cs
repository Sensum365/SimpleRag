using JetBrains.Annotations;
using SimpleRag.FileContent.Models;
using SimpleRag.Models;
using System.Text;

namespace SimpleRag.FileContent;

/// <summary>
/// Retrieves file content from the local file system.
/// </summary>
[PublicAPI]
public class FileContentLocalQuery : FileContentQuery
{
    internal async Task<Models.FileContent[]?> GetRawContentForSourceAsync(FileContentSourceLocal source, string fileExtensionType, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default)
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
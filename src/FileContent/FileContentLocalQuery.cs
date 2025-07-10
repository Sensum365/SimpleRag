using System.Text;
using JetBrains.Annotations;
using SimpleRag.FileContent.Models;

namespace SimpleRag.FileContent;

[UsedImplicitly]
/// <summary>
/// Retrieves file content from the local file system.
/// </summary>
public class FileContentLocalQuery : FileContentQuery
{
    internal async Task<Models.FileContent[]?> GetRawContentForSourceAsync(FileContentSourceLocal source, string fileExtensionType, CancellationToken cancellationToken = default)
    {
        SharedGuards(source);

        List<Models.FileContent> result = [];

        string[] files = Directory.GetFiles(source.Path, "*." + fileExtensionType, source.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        NotifyNumberOfFilesFound(files.Length);

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
            OnNotifyProgress("Parsing Local files from Disk", counter, files.Length);
            var pathWithoutRoot = path.Replace(source.Path, string.Empty);
            string content = await System.IO.File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
            result.Add(new Models.FileContent(path, content, pathWithoutRoot));
        }

        NotifyIgnoredFiles(ignoredFiles);

        return result.ToArray();
    }
}
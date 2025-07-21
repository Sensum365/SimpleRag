using JetBrains.Annotations;
using SimpleRag.DataProviders.Models;

namespace SimpleRag.DataProviders;

/// <summary>
/// A SourceProvider representing files on a local disk
/// </summary>
[PublicAPI]
public class LocalFilesDataProvider : IFileContentProvider
{
    /// <summary>
    /// Get the File Content of a source
    /// </summary>
    /// <param name="source">The File Source</param>
    /// <param name="onProgressNotification">Action for Progress Notification</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task<FileContent[]?> GetFileContent(FileContentSource source, Action<Notification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(source.Path))
        {
            throw new DataProviderException("Path is not defined");
        }

        List<FileContent> result = [];

        string[] files = Directory.GetFiles(source.Path, "*." + source.FileExtensionType, source.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        onProgressNotification?.Invoke(Notification.Create($"Found {files.Length} files"));

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
            onProgressNotification?.Invoke(Notification.Create("Parsing Local files from Disk", counter, files.Length));
            var pathWithoutRoot = path.Replace(source.Path, string.Empty);
            byte[] bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            result.Add(new FileContent(path, bytes, pathWithoutRoot));
        }

        if (ignoredFiles.Count > 0)
        {
            onProgressNotification?.Invoke(Notification.Create($"{ignoredFiles.Count} Files Ignored"));
        }

        return result.ToArray();
    }
}
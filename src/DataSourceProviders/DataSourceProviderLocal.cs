using SimpleRag.FileContent;
using SimpleRag.FileContent.Models;
using SimpleRag.Models;
using System.Text;

namespace SimpleRag.DataSourceProviders;

/// <summary>
/// A SourceProvider representing files on a local disk
/// </summary>
public class DataSourceProviderLocal : IDataSourceProvider
{
    public async Task<FileContent.Models.FileContent[]?> GetFileContent(FileContentSource source, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(source.Path))
        {
            throw new DataSourceProviderException("Path is not defined");
        }

        List<FileContent.Models.FileContent> result = [];

        string[] files = Directory.GetFiles(source.Path, "*." + source.FileExtensionType, source.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        onProgressNotification?.Invoke(ProgressNotification.Create($"Found {files.Length} files"));

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
            result.Add(new FileContent.Models.FileContent(path, content, pathWithoutRoot));
        }

        if (ignoredFiles.Count > 0)
        {
            onProgressNotification?.Invoke(ProgressNotification.Create($"{ignoredFiles.Count} Files Ignored"));
        }

        return result.ToArray();
    }
}
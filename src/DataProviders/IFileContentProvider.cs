using SimpleRag.DataProviders.Models;

namespace SimpleRag.DataProviders;

/// <summary>
/// Interface for a sourceProvider
/// </summary>
public interface IFileContentProvider
{
    Task<FileContent[]?> GetFileContent(FileContentSource source, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default);
}
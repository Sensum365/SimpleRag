namespace SimpleRag.DataProviders.Models;

/// <summary>
/// Interface for a sourceProvider
/// </summary>
public interface IFileContentProvider
{
    /// <summary>
    /// Get the File Content of a source
    /// </summary>
    /// <param name="source">The File Source</param>
    /// <param name="onProgressNotification">Action for Progress Notification</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<FileContent[]?> GetFileContent(FileContentSource source, Action<Notification>? onProgressNotification = null, CancellationToken cancellationToken = default);
}
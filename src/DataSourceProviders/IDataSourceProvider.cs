using SimpleRag.FileContent.Models;
using SimpleRag.Models;

namespace SimpleRag.DataSourceProviders;

/// <summary>
/// Interface for a sourceProvider
/// </summary>
public interface IDataSourceProvider
{
    Task<FileContent.Models.FileContent[]?> GetFileContent(FileContentSource source, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default);
}
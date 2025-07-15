using SimpleRag.DataSources.Models;
using SimpleRag.FileContent;
using SimpleRag.Models;

namespace SimpleRag.DataSources;

/// <summary>
/// Base Datasource Command
/// </summary>
/// <param name="fileContentQuery"></param>
public abstract class DataSourceCommand(FileContentQuery fileContentQuery)
{
    /// <summary>
    /// Get File Content for a source
    /// </summary>
    /// <param name="dataSource">The Datasource</param>
    /// <param name="fileExtension">The file Extension to use</param>
    /// <param name="onProgressNotification">Action on Progress Notification</param>
    /// <param name="cancellationToken">CancellationToken</param>
    protected async Task<FileContent.Models.FileContent[]?> GetFileContent(DataSource dataSource, string fileExtension, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        Guards(dataSource);
        return await fileContentQuery.GetRawContentForSourceAsync(dataSource.AsFileContentSource(), fileExtension, onProgressNotification, cancellationToken);
    }

    private static void Guards(DataSource source)
    {
        if (string.IsNullOrWhiteSpace(source.Path))
        {
            throw new SourceException("Source Path is not defined");
        }
    }
}
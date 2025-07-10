using JetBrains.Annotations;
using SimpleRag.DataSources.CSharp;
using SimpleRag.DataSources.CSharp.Models;
using SimpleRag.DataSources.Markdown;
using SimpleRag.DataSources.Markdown.Models;
using SimpleRag.DataSources.Models;
using SimpleRag.Models;

namespace SimpleRag;

/// <summary>
/// Coordinates ingestion of data sources.
/// </summary>
[PublicAPI]
public class Ingestion(CSharpDataSourceCommand cSharpDataSourceCommand, MarkdownDataSourceCommand markdownDataSourceCommand)
{
    /// <summary>
    /// Ingests the provided data sources.
    /// </summary>
    public async Task IngestAsync(IEnumerable<DataSource> dataSources, IngestionOptions? options = null, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (onProgressNotification != null)
            {
                cSharpDataSourceCommand.NotifyProgress += onProgressNotification;
                markdownDataSourceCommand.NotifyProgress += onProgressNotification;
            }

            foreach (DataSource source in dataSources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                switch (source)
                {
                    case CSharpDataSourceLocal cSharpDataSourceLocal:
                        await cSharpDataSourceCommand.IngestAsync(cSharpDataSourceLocal, options?.CSharpContentFormatBuilder, cancellationToken);
                        break;
                    case CSharpDataSourceGitHub cSharpDataSourceGitHub:
                        await cSharpDataSourceCommand.IngestAsync(cSharpDataSourceGitHub, options?.CSharpContentFormatBuilder, cancellationToken);
                        break;
                    case MarkdownDataSourceLocal markdownDataSourceLocal:
                        await markdownDataSourceCommand.IngestAsync(markdownDataSourceLocal, cancellationToken); //todo support content format builder
                        break;
                    case MarkdownDataSourceGitHub markdownDataSourceGitHub:
                        await markdownDataSourceCommand.IngestAsync(markdownDataSourceGitHub, cancellationToken); //todo support content format builder
                        break;
                }
            }
        }
        finally
        {
            if (onProgressNotification != null)
            {
                cSharpDataSourceCommand.NotifyProgress -= onProgressNotification;
                markdownDataSourceCommand.NotifyProgress -= onProgressNotification;
            }
        }
    }
}
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
public class Ingestion
{
    private readonly CSharpDataSourceCommand _cSharpDataSourceCommand;
    private readonly MarkdownDataSourceCommand _markdownDataSourceCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="Ingestion"/> class.
    /// </summary>
    public Ingestion(CSharpDataSourceCommand cSharpDataSourceCommand, MarkdownDataSourceCommand markdownDataSourceCommand)
    {
        _cSharpDataSourceCommand = cSharpDataSourceCommand;
        _markdownDataSourceCommand = markdownDataSourceCommand;
    }

    /// <summary>
    /// Ingests the provided data sources.
    /// </summary>
    public async Task IngestAsync(IEnumerable<DataSource> dataSources, IngestionOptions? options = null, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (onProgressNotification != null)
            {
                _cSharpDataSourceCommand.NotifyProgress += onProgressNotification;
                _markdownDataSourceCommand.NotifyProgress += onProgressNotification;
            }

            foreach (DataSource source in dataSources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                switch (source)
                {
                    case CSharpDataSourceLocal cSharpDataSourceLocal:
                        await _cSharpDataSourceCommand.IngestAsync(cSharpDataSourceLocal, options?.CSharpContentFormatBuilder, cancellationToken);
                        break;
                    case CSharpDataSourceGitHub cSharpDataSourceGitHub:
                        await _cSharpDataSourceCommand.IngestAsync(cSharpDataSourceGitHub, options?.CSharpContentFormatBuilder, cancellationToken);
                        break;
                    case MarkdownDataSourceLocal markdownDataSourceLocal:
                        await _markdownDataSourceCommand.IngestAsync(markdownDataSourceLocal, cancellationToken); //todo support content format builder
                        break;
                    case MarkdownDataSourceGitHub markdownDataSourceGitHub:
                        await _markdownDataSourceCommand.IngestAsync(markdownDataSourceGitHub, cancellationToken); //todo support content format builder
                        break;
                }
            }
        }
        finally
        {
            if (onProgressNotification != null)
            {
                _cSharpDataSourceCommand.NotifyProgress -= onProgressNotification;
                _markdownDataSourceCommand.NotifyProgress -= onProgressNotification;
            }
        }
    }
}
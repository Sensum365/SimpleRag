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
    public async Task IngestAsync(IEnumerable<DataSource> dataSources, IngestionOptions? options = null, CancellationToken cancellationToken = default)
    {
        dataSources = dataSources.ToList();
        string[] idCombos = dataSources.Select(x => x.CollectionId + " | " + x.Id).ToArray();
        if (idCombos.Length != idCombos.Distinct().Count())
        {
            throw new SourceException("One or more datasource CollectionId/SourceId combinations are not unique (which would result in them overwriting each other in the vector store)");
        }

        foreach (DataSource source in dataSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (source)
            {
                case CSharpDataSource cSharpDataSource:
                    await cSharpDataSourceCommand.IngestAsync(cSharpDataSource, options?.OnProgressNotification, cancellationToken);
                    break;
                case MarkdownDataSource markdownDataSource:
                    await markdownDataSourceCommand.IngestAsync(markdownDataSource, options?.OnProgressNotification, cancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataSources), "Unknown datasource");
            }
        }
    }
}
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
                case CSharpDataSourceLocal cSharpDataSourceLocal:
                    await cSharpDataSourceCommand.IngestAsync(cSharpDataSourceLocal, options?.OnProgressNotification, cancellationToken);
                    break;
                case CSharpDataSourceGitHub cSharpDataSourceGitHub:
                    await cSharpDataSourceCommand.IngestAsync(cSharpDataSourceGitHub, options?.OnProgressNotification, cancellationToken);
                    break;
                case MarkdownDataSourceLocal markdownDataSourceLocal:
                    await markdownDataSourceCommand.IngestAsync(markdownDataSourceLocal, options?.OnProgressNotification, cancellationToken);
                    break;
                case MarkdownDataSourceGitHub markdownDataSourceGitHub:
                    await markdownDataSourceCommand.IngestAsync(markdownDataSourceGitHub, options?.OnProgressNotification, cancellationToken);
                    break;
            }
        }
    }
}
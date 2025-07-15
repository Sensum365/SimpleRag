using JetBrains.Annotations;
using SimpleRag.DataSources;

namespace SimpleRag;

/// <summary>
/// Coordinates ingestion of data sources.
/// </summary>
[PublicAPI]
public class Ingestion
{
    /// <summary>
    /// Ingests the provided data sources.
    /// </summary>
    public async Task IngestAsync(IEnumerable<IDataSource> dataSources, IngestionOptions? options = null, CancellationToken cancellationToken = default)
    {
        dataSources = dataSources.ToList();
        string[] idCombos = dataSources.Select(x => x.CollectionId + " | " + x.Id).ToArray();
        if (idCombos.Length != idCombos.Distinct().Count())
        {
            throw new DataSourceException("One or more datasource CollectionId/SourceId combinations are not unique (which would result in them overwriting each other in the vector store)");
        }

        foreach (IDataSource source in dataSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await source.IngestAsync(options, cancellationToken);
        }
    }
}
using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SimpleRag.DataProviders.Models;
using SimpleRag.DataSources.PowerPoint.Chunker;
using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;

namespace SimpleRag.DataSources.PowerPoint;

/// <summary>
/// Represent a PowerPoint datasource
/// </summary>
[PublicAPI]
public class PowerPointDataSource : DataSourceFileBased
{
    private readonly IPowerPointChunker _chunker;
    private readonly IVectorStoreCommand _vectorStoreCommand;

    /// <summary>
    /// Represent a PowerPoint Datasource
    /// </summary>
    public PowerPointDataSource(IServiceProvider serviceProvider)
    {
        _chunker = serviceProvider.GetRequiredService<IPowerPointChunker>();
        _vectorStoreCommand = serviceProvider.GetRequiredService<IVectorStoreCommand>();
    }

    /// <summary>
    /// Represent a PowerPoint Datasource
    /// </summary>
    public PowerPointDataSource(IPowerPointChunker chunker, IVectorStoreCommand vectorStoreCommand)
    {
        _chunker = chunker;
        _vectorStoreCommand = vectorStoreCommand;
    }

    /// <summary>
    /// Builder of the desired format of the content to be vectorized or leave null to use the default provided format
    /// </summary>
    public Func<PowerPointChunk, string>? ContentFormatBuilder { get; set; }

    /// <summary>
    /// Ingest the datasource to the vector store
    /// </summary>
    /// <param name="ingestionOptions">Options for ingestion</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public override async Task IngestAsync(IngestionOptions? ingestionOptions = null, CancellationToken cancellationToken = default)
    {
        FileContent[]? files = await FilesProvider.GetFileContent(AsFileContentSource("pptx"), ingestionOptions?.OnProgressNotification, cancellationToken);
        if (files == null)
        {
            ingestionOptions?.ReportProgress("Nothing new to Ingest so skipping");
            return;
        }

        Func<PowerPointChunk, string> contentFormatBuilder = ContentFormatBuilder ?? (chunk =>
        {
            StringBuilder contentBuilder = new();
            contentBuilder.AppendLine($"<powerpoint_slide page_number=\"{chunk.Slide}\" total_pages=\"{chunk.TotalSlides}\" file_name=\"{chunk.Name}\" source_path=\"{chunk.SourcePath}\">");
            contentBuilder.AppendLine(chunk.Text);
            contentBuilder.AppendLine("</powerpoint_slide>");
            return contentBuilder.ToString();
        });

        List<VectorEntity> entities = [];
        int counter = 1;
        foreach (FileContent file in files)
        {
            ingestionOptions?.ReportProgress("Reading documents", counter, files.Length, file.PathWithoutRoot);
            PowerPointChunk[] chunks = _chunker.GetChunks(file);
            counter++;
            entities.AddRange(chunks.Select(chunk => new VectorEntity
            {
                SourceCollectionId = CollectionId.Value,
                SourceId = Id.Value,
                Id = Guid.NewGuid().ToString(),
                Content = contentFormatBuilder.Invoke(chunk),
                ContentKind = "PowerPointSlide",
                SourcePath = file.PathWithoutRoot,
                SourceKind = DataSourceKinds.PowerPoint,
                ContentId = null,
                ContentParent = null,
                ContentParentKind = null,
                ContentName = chunk.Name + "_slide" + chunk.Slide,
                ContentDependencies = null,
                ContentDescription = null,
                ContentReferences = null,
                ContentNamespace = null,
            }));
        }

        await _vectorStoreCommand.SyncAsync(this, entities, ingestionOptions?.OnProgressNotification, cancellationToken);
    }
}
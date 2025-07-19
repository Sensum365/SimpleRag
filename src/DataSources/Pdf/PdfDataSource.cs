using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SimpleRag.DataProviders.Models;
using SimpleRag.DataSources.Pdf.Chunker;
using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SimpleRag.DataSources.Pdf;

/// <summary>
/// Represent a PDF Datasource
/// </summary>
[PublicAPI]
public class PdfDataSource : DataSourceFileBased
{
    private readonly IPdfChunker _chunker;
    private readonly IVectorStoreCommand _vectorStoreCommand;

    /// <summary>
    /// Represent a PDF Datasource
    /// </summary>
    public PdfDataSource(IServiceProvider serviceProvider)
    {
        _chunker = serviceProvider.GetRequiredService<IPdfChunker>();
        _vectorStoreCommand = serviceProvider.GetRequiredService<IVectorStoreCommand>();
    }

    /// <summary>
    /// Represent a PDF Datasource
    /// </summary>
    public PdfDataSource(IPdfChunker chunker, IVectorStoreCommand vectorStoreCommand)
    {
        _chunker = chunker;
        _vectorStoreCommand = vectorStoreCommand;
    }

    /// <summary>The source kind handled by this command.</summary>
    public const string SourceKind = "PDF";

    /// <summary>
    /// Ingest the Datasource to the VectorStore
    /// </summary>
    /// <param name="ingestionOptions">Options for Ingestion</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public override async Task IngestAsync(IngestionOptions? ingestionOptions = null, CancellationToken cancellationToken = default)
    {
        FileContent[]? files = await base.FilesProvider.GetFileContent(AsFileContentSource("pdf"), ingestionOptions?.OnProgressNotification, cancellationToken);
        if (files == null)
        {
            ingestionOptions?.ReportProgress("Nothing new to Ingest so skipping");
            return;
        }

        List<VectorEntity> entities = [];

        int counter = 1;
        foreach (FileContent file in files)
        {
            ingestionOptions?.ReportProgress("Reading documents", counter, files.Length, file.PathWithoutRoot);
            PdfChunk[] chunks = _chunker.GetChunks(file);
            counter++;
            foreach (PdfChunk chunk in chunks)
            {
                StringBuilder content = new();
                content.AppendLine($"<pdf_page page_number=\"{chunk.Page}\" total_pages=\"{chunk.TotalPages}\" file_name=\"{chunk.Filename}\" folder=\"{chunk.Folder}\">");
                content.AppendLine(chunk.Text);
                content.AppendLine("</pdf_page>");

                entities.Add(new VectorEntity
                {
                    SourceCollectionId = CollectionId,
                    SourceId = Id,
                    Id = Guid.NewGuid().ToString(),
                    Content = content.ToString(),
                    ContentKind = "PDFPage",
                    SourcePath = file.PathWithoutRoot,
                    SourceKind = SourceKind,
                    ContentId = null,
                    ContentParent = null,
                    ContentParentKind = null,
                    ContentName = chunk.Filename + "_page" + chunk.Page,
                    ContentDependencies = null,
                    ContentDescription = null,
                    ContentReferences = null,
                    ContentNamespace = null,
                });
            }
        }

        await _vectorStoreCommand.SyncAsync(this, entities, ingestionOptions?.OnProgressNotification, cancellationToken);
    }
}
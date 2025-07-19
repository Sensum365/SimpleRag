using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SimpleRag.DataProviders.Models;
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
    private readonly IVectorStoreCommand _vectorStoreCommand;

    /// <summary>
    /// Represent a PDF Datasource
    /// </summary>
    public PdfDataSource(IServiceProvider serviceProvider)
    {
        _vectorStoreCommand = serviceProvider.GetRequiredService<IVectorStoreCommand>();
    }

    /// <summary>
    /// Represent a PDF Datasource
    /// </summary>
    public PdfDataSource(IVectorStoreCommand vectorStoreCommand)
    {
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
            counter++;
            PdfDocument document = PdfDocument.Open(file.Bytes);
            int pageNumber = 1;
            foreach (Page page in document.GetPages())
            {
                string pageText = page.Text;
                if (string.IsNullOrWhiteSpace(pageText))
                {
                    continue;
                }

                string filename = System.IO.Path.GetFileNameWithoutExtension(file.Path);
                StringBuilder content = new();
                content.AppendLine($"<pdf_page page_number=\"{pageNumber}\" total_pages=\"{document.NumberOfPages}\" file_name=\"{filename}\" folder=\"{file.PathWithoutRoot}\">");
                content.AppendLine(pageText);
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
                    ContentName = filename + "_page" + pageNumber,
                    ContentDependencies = null,
                    ContentDescription = null,
                    ContentReferences = null,
                    ContentNamespace = null,
                });
                pageNumber++;
            }
        }

        await _vectorStoreCommand.SyncAsync(this, entities, ingestionOptions?.OnProgressNotification, cancellationToken);
    }
}
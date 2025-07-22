using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SimpleRag.DataProviders.Models;
using SimpleRag.DataSources.Word.Chunker;
using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;

namespace SimpleRag.DataSources.Word;

/// <summary>
/// Represent a Word datasource
/// </summary>
[PublicAPI]
public class WordDataSource : DataSourceFileBased
{
    private readonly IWordChunker _chunker;
    private readonly IVectorStoreCommand _vectorStoreCommand;

    /// <summary>
    /// Represent a Word Datasource
    /// </summary>
    public WordDataSource(IServiceProvider serviceProvider)
    {
        _chunker = serviceProvider.GetRequiredService<IWordChunker>();
        _vectorStoreCommand = serviceProvider.GetRequiredService<IVectorStoreCommand>();
    }

    /// <summary>
    /// Represent a Word Datasource
    /// </summary>
    public WordDataSource(IWordChunker chunker, IVectorStoreCommand vectorStoreCommand)
    {
        _chunker = chunker;
        _vectorStoreCommand = vectorStoreCommand;
    }

    /// <summary>The source kind handled by this command.</summary>
    public const string SourceKind = "Word";

    /// <summary>
    /// Builder of the desired format of the content to be vectorized or leave null to use the default provided format
    /// </summary>
    public Func<WordChunk, string>? ContentFormatBuilder { get; set; }

    /// <summary>
    /// Ingest the datasource to the vector store
    /// </summary>
    /// <param name="ingestionOptions">Options for ingestion</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public override async Task IngestAsync(IngestionOptions? ingestionOptions = null, CancellationToken cancellationToken = default)
    {
        FileContent[]? files = await FilesProvider.GetFileContent(AsFileContentSource("docx"), ingestionOptions?.OnProgressNotification, cancellationToken);
        if (files == null)
        {
            ingestionOptions?.ReportProgress("Nothing new to Ingest so skipping");
            return;
        }

        Func<WordChunk, string> contentFormatBuilder = ContentFormatBuilder ?? (chunk =>
        {
            StringBuilder contentBuilder = new();
            contentBuilder.AppendLine($"<word_paragraph paragraph_number=\"{chunk.Paragraph}\" total_paragraphs=\"{chunk.TotalParagraphs}\" file_name=\"{chunk.Name}\" source_path=\"{chunk.SourcePath}\">");
            contentBuilder.AppendLine(chunk.Text);
            contentBuilder.AppendLine("</word_paragraph>");
            return contentBuilder.ToString();
        });

        List<VectorEntity> entities = [];
        int counter = 1;
        foreach (FileContent file in files)
        {
            ingestionOptions?.ReportProgress("Reading documents", counter, files.Length, file.PathWithoutRoot);
            WordChunk[] chunks = _chunker.GetChunks(file);
            counter++;
            entities.AddRange(chunks.Select(chunk => new VectorEntity
            {
                SourceCollectionId = CollectionId,
                SourceId = Id,
                Id = Guid.NewGuid().ToString(),
                Content = contentFormatBuilder.Invoke(chunk),
                ContentKind = "WordParagraph",
                SourcePath = file.PathWithoutRoot,
                SourceKind = SourceKind,
                ContentId = null,
                ContentParent = null,
                ContentParentKind = null,
                ContentName = chunk.Name + "_paragraph" + chunk.Paragraph,
                ContentDependencies = null,
                ContentDescription = null,
                ContentReferences = null,
                ContentNamespace = null,
            }));
        }

        await _vectorStoreCommand.SyncAsync(this, entities, ingestionOptions?.OnProgressNotification, cancellationToken);
    }
}

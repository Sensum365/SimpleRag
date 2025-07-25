using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SimpleRag.DataProviders.Models;
using SimpleRag.DataSources.Markdown.Chunker;
using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;
using System.Text.RegularExpressions;

namespace SimpleRag.DataSources.Markdown;

/// <summary>
/// Class for markdown sources.
/// </summary>
[PublicAPI]
public class MarkdownDataSource : DataSourceFileBased
{
    private readonly IMarkdownChunker _chunker;
    private readonly IVectorStoreCommand _vectorStoreCommand;

    /// <summary>
    /// Class for markdown sources.
    /// </summary>
    public MarkdownDataSource(IMarkdownChunker chunker, IVectorStoreCommand vectorStoreCommand)
    {
        _chunker = chunker;
        _vectorStoreCommand = vectorStoreCommand;
    }

    /// <summary>
    /// Class for markdown sources.
    /// </summary>
    public MarkdownDataSource(IServiceProvider serviceProvider)
    {
        _chunker = serviceProvider.GetRequiredService<IMarkdownChunker>();
        _vectorStoreCommand = serviceProvider.GetRequiredService<IVectorStoreCommand>();
    }

    /// <summary>Gets or sets a value indicating whether HTML comments should be ignored.</summary>
    public bool IgnoreCommentedOutContent { get; init; } = true;

    /// <summary>Gets or sets a value indicating whether image references should be ignored.</summary>
    public bool IgnoreImages { get; init; } = true;

    /// <summary>Gets or sets the line count threshold for chunking files.</summary>
    public int? OnlyChunkIfMoreThanThisNumberOfLines { get; init; } = 25;

    /// <summary>Gets or sets the heading levels to chunk at.</summary>
    public int LevelsToChunk { get; init; } = 2;

    /// <summary>Gets or sets patterns for lines to ignore when chunking.</summary>
    public string? ChunkLineIgnorePatterns { get; init; }

    /// <summary>Gets or sets the minimum size of a chunk in characters.</summary>
    public int? IgnoreChunkIfLessThanThisAmountOfChars { get; init; } = 25;


    /// <summary>
    /// Builder of the desired format of the Content to be vectorized or leave null to use the default provided format
    /// </summary>
    public Func<MarkdownChunk, string>? ContentFormatBuilder { get; set; }

    /// <summary>
    /// Ingest a Markdown Source
    /// </summary>
    /// <param name="ingestionOptions"></param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public override async Task IngestAsync(IngestionOptions? ingestionOptions = null, CancellationToken cancellationToken = default)
    {
        FileContent[]? files = await FilesProvider.GetFileContent(AsFileContentSource("md"), ingestionOptions?.OnProgressNotification, cancellationToken);
        if (files == null)
        {
            ingestionOptions?.ReportProgress("Nothing new to Ingest so skipping");
            return;
        }

        var newLine = Environment.NewLine;
        Func<MarkdownChunk, string> contentFormatBuilder = ContentFormatBuilder ?? (chunk =>
        {
            StringBuilder contentBuilder = new();
            contentBuilder.AppendLine($"<markdown name=\"{chunk.Name}\" source=\"{System.IO.Path.GetFileNameWithoutExtension(chunk.SourcePath)}\">");
            contentBuilder.AppendLine(chunk.Content);
            contentBuilder.AppendLine("</markdown>");
            return contentBuilder.ToString();
        });

        List<VectorEntity> entries = [];

        foreach (var file in files)
        {
            string fileContent = file.GetContentAsUtf8String();
            var numberOfLine = fileContent.Split(["\n"], StringSplitOptions.RemoveEmptyEntries).Length;
            if (IgnoreFileIfMoreThanThisNumberOfLines.HasValue && numberOfLine > IgnoreFileIfMoreThanThisNumberOfLines)
            {
                continue;
            }

            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(file.Path);
            if (IgnoreCommentedOutContent)
            {
                //Remove Any Commented out parts
                fileContent = Regex.Replace(fileContent, "<!--[\\s\\S]*?-->", string.Empty);
            }

            if (IgnoreImages)
            {
                //Remove Any Images
                fileContent = Regex.Replace(fileContent, @"!\[.*?\]\(.*?\)", string.Empty);
            }

            fileContent = Regex.Replace(fileContent, @"\r\n[\r\n]+|\r[\r]+|\n[\n]+", newLine + newLine);
            fileContent = fileContent.Trim();

            if (numberOfLine > OnlyChunkIfMoreThanThisNumberOfLines)
            {
                //Chunk larger files
                MarkdownChunk[] chunks = _chunker.GetChunks(fileContent,
                    LevelsToChunk,
                    ChunkLineIgnorePatterns,
                    IgnoreChunkIfLessThanThisAmountOfChars);

                foreach (MarkdownChunk chunk in chunks)
                {
                    chunk.SourcePath = file.PathWithoutRoot;
                }

                entries.AddRange(chunks.Select(x => new VectorEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    ContentKind = "Markdown",
                    Content = contentFormatBuilder.Invoke(x),
                    ContentId = x.ChunkId,
                    ContentName = x.Name,
                    SourceId = Id.Value,
                    SourceKind = DataSourceKinds.Markdown,
                    SourceCollectionId = CollectionId.Value,
                    SourcePath = file.PathWithoutRoot,
                    ContentParent = null,
                    ContentParentKind = null,
                    ContentNamespace = null,
                    ContentDependencies = null,
                    ContentDescription = null,
                    ContentReferences = null
                }));
            }
            else
            {
                entries.Add(new VectorEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceId = Id.Value,
                    SourceKind = DataSourceKinds.Markdown,
                    SourceCollectionId = CollectionId.Value,
                    SourcePath = file.PathWithoutRoot,
                    ContentKind = "Markdown",
                    Content = $"{fileNameWithoutExtension}{newLine}---{newLine}{fileContent}", //todo - support Content format builder
                    ContentName = fileNameWithoutExtension,
                    ContentId = null,
                    ContentParent = null,
                    ContentParentKind = null,
                    ContentNamespace = null,
                    ContentDependencies = null,
                    ContentDescription = null,
                    ContentReferences = null
                });
            }
        }

        await _vectorStoreCommand.SyncAsync(this, entries, ingestionOptions?.OnProgressNotification, cancellationToken);
        ingestionOptions?.ReportProgress("Done");
    }
}
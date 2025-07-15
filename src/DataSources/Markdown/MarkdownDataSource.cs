using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using SimpleRag.DataProviders.Models;
using SimpleRag.DataSources.Markdown.Chunker;

namespace SimpleRag.DataSources.Markdown;

/// <summary>
/// Class for markdown sources.
/// </summary>
public class MarkdownDataSource : DataSourceFileBased
{
    private readonly IMarkdownChunker _chunker;
    private readonly IVectorStoreQuery _vectorStoreQuery;
    private readonly IVectorStoreCommand _vectorStoreCommand;

    /// <summary>
    /// Class for markdown sources.
    /// </summary>
    public MarkdownDataSource(IMarkdownChunker chunker, IVectorStoreQuery vectorStoreQuery, IVectorStoreCommand vectorStoreCommand)
    {
        _chunker = chunker;
        _vectorStoreQuery = vectorStoreQuery;
        _vectorStoreCommand = vectorStoreCommand;
    }

    /// <summary>
    /// Class for markdown sources.
    /// </summary>
    public MarkdownDataSource(IServiceProvider serviceProvider)
    {
        _chunker = serviceProvider.GetRequiredService<IMarkdownChunker>();
        _vectorStoreQuery = serviceProvider.GetRequiredService<IVectorStoreQuery>();
        _vectorStoreCommand = serviceProvider.GetRequiredService<IVectorStoreCommand>();
    }

    /// <summary>The source kind handled by this command.</summary>
    public const string SourceKind = "Markdown";

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

    //todo support content format builder

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

        List<VectorEntity> entries = [];

        foreach (var file in files)
        {
            var numberOfLine = file.Content.Split(["\n"], StringSplitOptions.RemoveEmptyEntries).Length;
            if (IgnoreFileIfMoreThanThisNumberOfLines.HasValue && numberOfLine > IgnoreFileIfMoreThanThisNumberOfLines)
            {
                continue;
            }

            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(file.Path);
            var content = file.Content;
            if (IgnoreCommentedOutContent)
            {
                //Remove Any Commented out parts
                content = Regex.Replace(content, "<!--[\\s\\S]*?-->", string.Empty);
            }

            if (IgnoreImages)
            {
                //Remove Any Images
                content = Regex.Replace(content, @"!\[.*?\]\(.*?\)", string.Empty);
            }

            var newLine = Environment.NewLine;
            content = Regex.Replace(content, @"\r\n[\r\n]+|\r[\r]+|\n[\n]+", newLine + newLine);
            content = content.Trim();

            if (numberOfLine > OnlyChunkIfMoreThanThisNumberOfLines)
            {
                //Chunk larger files
                MarkdownChunk[] chunks = _chunker.GetChunks(content,
                    LevelsToChunk,
                    ChunkLineIgnorePatterns,
                    IgnoreChunkIfLessThanThisAmountOfChars);

                entries.AddRange(chunks.Select(x => new VectorEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    ContentKind = "Markdown",
                    Content = $"{fileNameWithoutExtension} - {x.Name}{newLine}---{newLine}{x.Content}", //todo - support Content format builder
                    ContentId = x.ChunkId,
                    ContentName = x.Name,
                    SourceId = Id,
                    SourceKind = SourceKind,
                    SourceCollectionId = CollectionId,
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
                    SourceId = Id,
                    SourceKind = SourceKind,
                    SourceCollectionId = CollectionId,
                    SourcePath = file.PathWithoutRoot,
                    ContentKind = "Markdown",
                    Content = $"{fileNameWithoutExtension}{newLine}---{newLine}{content}", //todo - support Content format builder
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

        var existingData = await _vectorStoreQuery.GetExistingAsync(x => x.SourceCollectionId == CollectionId && x.SourceId == Id, cancellationToken);

        int counter = 0;
        List<string> idsToKeep = [];
        foreach (var entity in entries)
        {
            counter++;

            ingestionOptions?.ReportProgress("Embedding Data", counter, entries.Count);
            var existing = existingData.FirstOrDefault(x => x.GetContentCompareKey() == entity.GetContentCompareKey());
            if (existing == null)
            {
                await RetryHelper.ExecuteWithRetryAsync(async () => { await _vectorStoreCommand.UpsertAsync(entity, cancellationToken); }, 3, TimeSpan.FromSeconds(30));
            }
            else
            {
                idsToKeep.Add(existing.Id);
            }
        }

        var idsToDelete = existingData.Select(x => x.Id).Except(idsToKeep).ToList();
        if (idsToDelete.Count != 0)
        {
            ingestionOptions?.ReportProgress($"Removing {idsToDelete.Count} entities that are no longer in source");
            await _vectorStoreCommand.DeleteAsync(idsToDelete, cancellationToken);
        }

        ingestionOptions?.ReportProgress("Done");
    }
}
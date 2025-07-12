using JetBrains.Annotations;
using SimpleRag.DataSources.Markdown.Models;
using SimpleRag.DataSources.Models;
using SimpleRag.FileContent;
using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;
using System.Text.RegularExpressions;
using SimpleRag.Helpers;
using SimpleRag.Models;

namespace SimpleRag.DataSources.Markdown;

/// <summary>
/// Command used for ingesting markdown based data sources.
/// </summary>
[PublicAPI]
public class MarkdownDataSourceCommand(
    MarkdownChunker chunker,
    VectorStoreQuery vectorStoreQuery,
    VectorStoreCommand vectorStoreCommand,
    FileContentGitHubQuery gitHubFileContentQuery,
    FileContentLocalQuery localFileContentQuery)
{
    /// <summary>The source kind handled by this command.</summary>
    public const string SourceKind = "Markdown";

    /// <summary>
    /// Ingests a local markdown source.
    /// </summary>
    /// <param name="dataSource">The source to ingest.</param>
    /// <param name="onProgressNotification">Notification</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task IngestAsync(MarkdownDataSourceLocal dataSource, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        Guards(dataSource);

        FileContent.Models.FileContent[]? rawFiles = await localFileContentQuery.GetRawContentForSourceAsync(dataSource.AsFileContentSource(), "md", onProgressNotification, cancellationToken);
        if (rawFiles == null)
        {
            onProgressNotification?.Invoke(ProgressNotification.Create("Nothing new to Ingest so skipping"));
            return;
        }

        await IngestAsync(dataSource, rawFiles, onProgressNotification, cancellationToken);
    }

    /// <summary>
    /// Ingests a GitHub based markdown source.
    /// </summary>
    /// <param name="dataSource">The source to ingest.</param>
    /// <param name="onProgressNotification">Notification</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task IngestAsync(MarkdownDataSourceGitHub dataSource, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        Guards(dataSource);

        FileContent.Models.FileContent[]? rawFiles = await gitHubFileContentQuery.GetRawContentForSourceAsync(dataSource.AsFileContentSource(), "md", onProgressNotification, cancellationToken);
        if (rawFiles == null)
        {
            onProgressNotification?.Invoke(ProgressNotification.Create("Nothing new to Ingest so skipping"));
            return;
        }

        await IngestAsync(dataSource, rawFiles, onProgressNotification, cancellationToken);
    }

    private async Task IngestAsync(MarkdownDataSource dataSource, FileContent.Models.FileContent[] rawFiles, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        List<VectorEntity> entries = [];

        foreach (var rawFile in rawFiles)
        {
            var numberOfLine = rawFile.Content.Split(["\n"], StringSplitOptions.RemoveEmptyEntries).Length;
            if (dataSource.IgnoreFileIfMoreThanThisNumberOfLines.HasValue && numberOfLine > dataSource.IgnoreFileIfMoreThanThisNumberOfLines)
            {
                continue;
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(rawFile.Path);
            var content = rawFile.Content;
            if (dataSource.IgnoreCommentedOutContent)
            {
                //Remove Any Commented out parts
                content = Regex.Replace(content, "<!--[\\s\\S]*?-->", string.Empty);
            }

            if (dataSource.IgnoreImages)
            {
                //Remove Any Images
                content = Regex.Replace(content, @"!\[.*?\]\(.*?\)", string.Empty);
            }

            var newLine = Environment.NewLine;
            content = Regex.Replace(content, @"\r\n[\r\n]+|\r[\r]+|\n[\n]+", newLine + newLine);
            content = content.Trim();

            if (numberOfLine > dataSource.OnlyChunkIfMoreThanThisNumberOfLines)
            {
                //Chunk larger files
                MarkdownChunk[] chunks = chunker.GetChunks(content,
                    dataSource.LevelsToChunk,
                    dataSource.ChunkLineIgnorePatterns,
                    dataSource.IgnoreChunkIfLessThanThisAmountOfChars);

                entries.AddRange(chunks.Select(x => new VectorEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    ContentKind = "Markdown",
                    Content = $"{fileNameWithoutExtension} - {x.Name}{newLine}---{newLine}{x.Content}", //todo - support Content format builder
                    ContentId = x.ChunkId,
                    ContentName = x.Name,
                    SourceId = dataSource.Id,
                    SourceKind = SourceKind,
                    SourceCollectionId = dataSource.CollectionId,
                    SourcePath = rawFile.PathWithoutRoot,
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
                    SourceId = dataSource.Id,
                    SourceKind = SourceKind,
                    SourceCollectionId = dataSource.CollectionId,
                    SourcePath = rawFile.PathWithoutRoot,
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

        var existingData = await vectorStoreQuery.GetExistingAsync(x => x.SourceCollectionId == dataSource.CollectionId && x.SourceId == dataSource.Id, cancellationToken);

        int counter = 0;
        List<string> idsToKeep = [];
        foreach (var entity in entries)
        {
            counter++;

            onProgressNotification?.Invoke(ProgressNotification.Create("Embedding Data", counter, entries.Count));
            var existing = existingData.FirstOrDefault(x => x.GetContentCompareKey() == entity.GetContentCompareKey());
            if (existing == null)
            {
                await RetryHelper.ExecuteWithRetryAsync(async () => { await vectorStoreCommand.UpsertAsync(entity, cancellationToken); }, 3, TimeSpan.FromSeconds(30));
            }
            else
            {
                idsToKeep.Add(existing.Id);
            }
        }

        var idsToDelete = existingData.Select(x => x.Id).Except(idsToKeep).ToList();
        if (idsToDelete.Count != 0)
        {
            onProgressNotification?.Invoke(ProgressNotification.Create($"Removing {idsToDelete.Count} entities that are no longer in source"));
            await vectorStoreCommand.DeleteAsync(idsToDelete, cancellationToken);
        }

        onProgressNotification?.Invoke(ProgressNotification.Create("Done"));
    }


    private static void Guards(DataSource source)
    {
        if (string.IsNullOrWhiteSpace(source.Path))
        {
            throw new SourceException("Source Path is not defined");
        }
    }
}
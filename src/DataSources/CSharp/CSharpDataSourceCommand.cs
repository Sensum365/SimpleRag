using System.Text;
using JetBrains.Annotations;
using SimpleRag.DataSources.CSharp.Models;
using SimpleRag.DataSources.Models;
using SimpleRag.FileContent;
using SimpleRag.Helpers;
using SimpleRag.Models;
using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;

namespace SimpleRag.DataSources.CSharp;

/// <summary>
/// Command class for ingesting and processing C# data sources, supporting both local and GitHub-based sources.
/// Handles chunking, formatting, and storing code entities as vector embeddings for semantic search and retrieval.
/// </summary>
/// <param name="chunker">The CSharpChunker used to extract code entities from C# source files.</param>
/// <param name="vectorStoreCommand">The command for upserting and deleting vector entities in the vector store.</param>
/// <param name="vectorStoreQuery">The query service for retrieving existing vector entities from the vector store.</param>
/// <param name="gitHubFilesQuery">The query service for retrieving C# files from GitHub sources.</param>
/// <param name="localFilesQuery">The query service for retrieving C# files from local sources.</param>
[PublicAPI]
public class CSharpDataSourceCommand(
    CSharpChunker chunker,
    VectorStoreCommand vectorStoreCommand,
    VectorStoreQuery vectorStoreQuery,
    FileContentGitHubQuery gitHubFilesQuery,
    FileContentLocalQuery localFilesQuery) : ProgressNotificationBase
{
    /// <summary>
    /// The sourceKind this command ingest
    /// </summary>
    public const string SourceKind = "CSharp";

    /// <summary>
    /// Ingest a Local C# Source
    /// </summary>
    /// <param name="source">The Source to ingest</param>
    /// <param name="contentFormatBuilder">Builder of the desired format of the Content to be vectorized or leave null to use the default provided format</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task IngestAsync(CSharpDataSourceLocal source, Func<CSharpChunk, string>? contentFormatBuilder = null, CancellationToken cancellationToken = default)
    {
        Guards(source);
        localFilesQuery.NotifyProgress += OnNotifyProgress;
        try
        {
            FileContent.Models.FileContent[]? files = await localFilesQuery.GetRawContentForSourceAsync(source.AsFileContentSource(), "cs", cancellationToken);
            if (files == null)
            {
                OnNotifyProgress("Nothing new to Ingest so skipping");
                return;
            }

            await IngestAsync(source, contentFormatBuilder, files, cancellationToken);
        }
        finally
        {
            localFilesQuery.NotifyProgress -= OnNotifyProgress;
        }
    }

    /// <summary>
    /// Ingest a GitHub C# Source
    /// </summary>
    /// <param name="source">The Source to ingest</param>
    /// <param name="contentFormatBuilder">Builder of the desired format of the Content to be vectorized or leave null to use the default provided format</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task IngestAsync(CSharpDataSourceGitHub source, Func<CSharpChunk, string>? contentFormatBuilder = null, CancellationToken cancellationToken = default)
    {
        Guards(source);
        gitHubFilesQuery.NotifyProgress += OnNotifyProgress;
        try
        {
            FileContent.Models.FileContent[]? files = await gitHubFilesQuery.GetRawContentForSourceAsync(source.AsFileContentSource(), "cs", cancellationToken);
            if (files == null)
            {
                OnNotifyProgress("Nothing new to Ingest so skipping");
                return;
            }

            await IngestAsync(source, contentFormatBuilder, files, cancellationToken);
        }
        finally
        {
            gitHubFilesQuery.NotifyProgress -= OnNotifyProgress;
        }
    }

    private async Task IngestAsync(DataSource source, Func<CSharpChunk, string>? contentFormatBuilder, FileContent.Models.FileContent[] files, CancellationToken cancellationToken = default)
    {
        List<CSharpChunk> codeEntities = [];

        foreach (FileContent.Models.FileContent file in files)
        {
            var numberOfLine = file.Content.Split(["\n"], StringSplitOptions.RemoveEmptyEntries).Length;
            if (source.IgnoreFileIfMoreThanThisNumberOfLines.HasValue && numberOfLine > source.IgnoreFileIfMoreThanThisNumberOfLines)
            {
                continue;
            }

            List<CSharpChunk> entitiesForFile = chunker.GetCodeEntities(file.Content);
            foreach (CSharpChunk codeEntity in entitiesForFile)
            {
                codeEntity.SourcePath = file.PathWithoutRoot;
            }

            codeEntities.AddRange(entitiesForFile);
        }

        OnNotifyProgress($"{files.Length} Files was transformed into {codeEntities.Count} Code Entities for Vector Import. Preparing Embedding step...");

        contentFormatBuilder ??= chunk =>
        {
            StringBuilder sb = new();
            string parentDetails = string.Empty;
            if (!string.IsNullOrWhiteSpace(chunk.Parent))
            {
                parentDetails = $" parentKind=\"{chunk.ParentKindAsString}\" parent=\"{chunk.Parent}\"";
            }

            sb.AppendLine($"<code name=\"{chunk.Name}\" kind=\"{chunk.KindAsString}\" namespace=\"{chunk.Namespace}\"{parentDetails}>");
            sb.AppendLine(chunk.XmlSummary + " " + chunk.Value);
            if (chunk.Dependencies.Count > 0)
            {
                sb.AppendLine("<dependencies>");
                sb.AppendLine(string.Join(Environment.NewLine, chunk.Dependencies.Select(x => "- " + x)));
                sb.AppendLine("</dependencies>");
            }

            if (chunk.References is { Count: > 0 })
            {
                sb.AppendLine("<used_by>");
                sb.AppendLine(string.Join(Environment.NewLine, chunk.References.Select(x => "- " + x.Path)));
                sb.AppendLine("</used_by>");
            }

            sb.AppendLine("</code>");
            return sb.ToString();
        };

        //Creating References
        foreach (CSharpChunk codeEntity in codeEntities)
        {
            switch (codeEntity.Kind)
            {
                case CSharpKind.Enum:
                case CSharpKind.Interface:
                case CSharpKind.Constructor:
                case CSharpKind.Class:
                case CSharpKind.Struct:
                case CSharpKind.Record:
                    codeEntity.References = codeEntities.Where(x => x != codeEntity && x.Dependencies.Any(y => y == codeEntity.Name)).ToList();
                    break;
            }
        }

        VectorEntity[] existingData = await vectorStoreQuery.GetExistingAsync(x => x.SourceId == source.Id, cancellationToken);

        int counter = 0;
        List<string> idsToKeep = [];
        foreach (CSharpChunk codeEntity in codeEntities)
        {
            counter++;

            OnNotifyProgress("Embedding Data", counter, codeEntities.Count);

            string content = contentFormatBuilder.Invoke(codeEntity);

            VectorEntity entity = new()
            {
                Id = Guid.NewGuid().ToString(),
                SourceId = source.Id,
                ContentId = null,
                SourceCollectionId = source.CollectionId,
                SourceKind = SourceKind,
                SourcePath = codeEntity.SourcePath,
                ContentKind = codeEntity.KindAsString,
                ContentParent = codeEntity.Parent,
                ContentParentKind = codeEntity.ParentKindAsString,
                ContentName = codeEntity.Name,
                ContentNamespace = codeEntity.Namespace,
                ContentDependencies = codeEntity.Dependencies.Count == 0 ? null : string.Join(";", codeEntity.Dependencies),
                ContentReferences = codeEntity.References is { Count: 0 } ? null : string.Join(";", codeEntity.References?.Select(x => x.Path) ?? []),
                ContentDescription = codeEntity.XmlSummary,
                Content = content,
            };

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
            OnNotifyProgress($"Removing {idsToDelete.Count} entities that are no longer in source...");
            await vectorStoreCommand.DeleteAsync(idsToDelete, cancellationToken);
        }

        OnNotifyProgress("Done");
    }

    private static void Guards(DataSource source)
    {
        if (string.IsNullOrWhiteSpace(source.Path))
        {
            throw new SourceException("Source Path is not defined");
        }
    }
}
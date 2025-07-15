using JetBrains.Annotations;
using SimpleRag.DataSources.CSharp.Models;
using SimpleRag.DataSources.Models;
using SimpleRag.FileContent;
using SimpleRag.Helpers;
using SimpleRag.Models;
using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;
using System.Text;

namespace SimpleRag.DataSources.CSharp;

/// <summary>
/// Command class for ingesting and processing C# data sources, supporting both local and GitHub-based sources.
/// Handles chunking, formatting, and storing code entities as vector embeddings for semantic search and retrieval.
/// </summary>
/// <param name="chunker">The CSharpChunker used to extract code entities from C# source files.</param>
/// <param name="vectorStoreCommand">The command for upserting and deleting vector entities in the vector store.</param>
/// <param name="vectorStoreQuery">The query service for retrieving existing vector entities from the vector store.</param>
/// <param name="fileContentQuery">The query to retrieve raw files</param>
[PublicAPI]
public class CSharpDataSourceCommand(
    CSharpChunker chunker,
    VectorStoreCommand vectorStoreCommand,
    VectorStoreQuery vectorStoreQuery,
    FileContentQuery fileContentQuery) : DataSourceCommand(fileContentQuery)
{
    /// <summary>
    /// The sourceKind this command ingest
    /// </summary>
    public const string SourceKind = "CSharp";

    /// <summary>
    /// Ingest a C# Source
    /// </summary>
    /// <param name="dataSource">The Datasource</param>
    /// <param name="onProgressNotification">Action to execute on progress notification</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task IngestAsync(CSharpDataSource dataSource, Action<ProgressNotification>? onProgressNotification = null, CancellationToken cancellationToken = default)
    {
        FileContent.Models.FileContent[]? files = await GetFileContent(dataSource, "cs", onProgressNotification, cancellationToken);
        if (files == null)
        {
            onProgressNotification?.Invoke(ProgressNotification.Create("Nothing new to Ingest so skipping"));
            return;
        }

        List<CSharpChunk> codeEntities = [];

        foreach (FileContent.Models.FileContent file in files)
        {
            var numberOfLine = file.Content.Split(["\n"], StringSplitOptions.RemoveEmptyEntries).Length;
            if (dataSource.IgnoreFileIfMoreThanThisNumberOfLines.HasValue && numberOfLine > dataSource.IgnoreFileIfMoreThanThisNumberOfLines)
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

        onProgressNotification?.Invoke(ProgressNotification.Create($"{files.Length} Files was transformed into {codeEntities.Count} Code Entities for Vector Import. Preparing Embedding step..."));

        Func<CSharpChunk, string>? cSharpContentFormatBuilder = dataSource.CSharpContentFormatBuilder;
        if (cSharpContentFormatBuilder == null)
        {
            cSharpContentFormatBuilder = chunk =>
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

            VectorEntity[] existingData = await vectorStoreQuery.GetExistingAsync(x => x.SourceCollectionId == dataSource.CollectionId && x.SourceId == dataSource.Id, cancellationToken);

            int counter = 0;
            List<string> idsToKeep = [];

            foreach (CSharpChunk codeEntity in codeEntities)
            {
                counter++;

                onProgressNotification?.Invoke(ProgressNotification.Create("Embedding Data", counter, codeEntities.Count));

                string content = cSharpContentFormatBuilder.Invoke(codeEntity);

                VectorEntity entity = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceId = dataSource.Id,
                    ContentId = null,
                    SourceCollectionId = dataSource.CollectionId,
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

                string contentCompareKey = entity.GetContentCompareKey();
                var existing = existingData.FirstOrDefault(x => x.GetContentCompareKey() == contentCompareKey);
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
                onProgressNotification?.Invoke(ProgressNotification.Create($"Removing {idsToDelete.Count} entities that are no longer in source..."));
                await vectorStoreCommand.DeleteAsync(idsToDelete, cancellationToken);
            }

            onProgressNotification?.Invoke(ProgressNotification.Create("Done"));
        }
    }
}
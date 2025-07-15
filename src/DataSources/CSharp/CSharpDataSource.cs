using SimpleRag.DataSources.CSharp.Models;
using SimpleRag.Helpers;
using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;
using System.Text;

namespace SimpleRag.DataSources.CSharp;

/// <summary>
/// Represent a C# Based Datasource
/// </summary>
public class CSharpDataSource(ICSharpChunker chunker, IVectorStoreQuery vectorStoreQuery, IVectorStoreCommand vectorStoreCommand) : FileBasedDataSource
{
    /// <summary>
    /// The sourceKind this command ingest
    /// </summary>
    public const string SourceKind = "CSharp";

    /// <summary>
    /// Builder of the desired format of the Content to be vectorized or leave null to use the default provided format
    /// </summary>
    public Func<CSharpChunk, string>? CSharpContentFormatBuilder { get; set; }

    /// <summary>
    /// Ingest a C# Source
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public override async Task IngestAsync(IngestionOptions ingestionOptions = null, CancellationToken cancellationToken = default)
    {
        DataProviders.Models.FileContent[]? files = await FilesProvider.GetFileContent(AsFileContentSource("cs"), ingestionOptions.OnProgressNotification, cancellationToken);
        if (files == null)
        {
            ingestionOptions.OnProgressNotification?.Invoke(ProgressNotification.Create("Nothing new to Ingest so skipping"));
            return;
        }

        List<CSharpChunk> codeEntities = [];

        foreach (DataProviders.Models.FileContent file in files)
        {
            var numberOfLine = file.Content.Split(["\n"], StringSplitOptions.RemoveEmptyEntries).Length;
            if (IgnoreFileIfMoreThanThisNumberOfLines.HasValue && numberOfLine > IgnoreFileIfMoreThanThisNumberOfLines)
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

        ingestionOptions.OnProgressNotification?.Invoke(ProgressNotification.Create($"{files.Length} Files was transformed into {codeEntities.Count} Code Entities for Vector Import. Preparing Embedding step..."));

        Func<CSharpChunk, string>? cSharpContentFormatBuilder = CSharpContentFormatBuilder;
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

            VectorEntity[] existingData = await vectorStoreQuery.GetExistingAsync(x => x.SourceCollectionId == CollectionId && x.SourceId == Id, cancellationToken);

            int counter = 0;
            List<string> idsToKeep = [];

            foreach (CSharpChunk codeEntity in codeEntities)
            {
                counter++;

                ingestionOptions.OnProgressNotification?.Invoke(ProgressNotification.Create("Embedding Data", counter, codeEntities.Count));

                string content = cSharpContentFormatBuilder.Invoke(codeEntity);

                VectorEntity entity = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceId = Id,
                    ContentId = null,
                    SourceCollectionId = CollectionId,
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
                ingestionOptions.OnProgressNotification?.Invoke(ProgressNotification.Create($"Removing {idsToDelete.Count} entities that are no longer in source..."));
                await vectorStoreCommand.DeleteAsync(idsToDelete, cancellationToken);
            }

            ingestionOptions.OnProgressNotification?.Invoke(ProgressNotification.Create("Done"));
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using SimpleRag.DataSources.CSharp.Chunker;
using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;
using System.Text;
using JetBrains.Annotations;

namespace SimpleRag.DataSources.CSharp;

/// <summary>
/// Represent a C# Based Datasource
/// </summary>
[PublicAPI]
public class CSharpDataSource : DataSourceFileBased
{
    private readonly ICSharpChunker _chunker;
    private readonly IVectorStoreCommand _vectorStoreCommand;

    /// <summary>
    /// Options for the CSharpChunker
    /// </summary>
    public CSharpChunkerOptions? ChunkerOptions { get; set; }

    /// <summary>
    /// Represent a C# Based Datasource
    /// </summary>
    public CSharpDataSource(ICSharpChunker chunker, IVectorStoreCommand vectorStoreCommand)
    {
        _chunker = chunker;
        _vectorStoreCommand = vectorStoreCommand;
    }

    /// <summary>
    /// Represent a C# Based Datasource
    /// </summary>
    public CSharpDataSource(IServiceProvider serviceProvider)
    {
        _chunker = serviceProvider.GetRequiredService<ICSharpChunker>();
        _vectorStoreCommand = serviceProvider.GetRequiredService<IVectorStoreCommand>();
    }

    /// <summary>
    /// Builder of the desired format of the Content to be vectorized or leave null to use the default provided format
    /// </summary>
    public Func<CSharpChunk, string>? ContentFormatBuilder { get; set; }

    /// <summary>
    /// Ingest a C# Source
    /// </summary>
    /// <param name="ingestionOptions">Options for the ingestion</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public override async Task IngestAsync(IngestionOptions? ingestionOptions = null, CancellationToken cancellationToken = default)
    {
        DataProviders.Models.FileContent[]? files = await FilesProvider.GetFileContent(AsFileContentSource("cs"), ingestionOptions?.OnProgressNotification, cancellationToken);
        if (files == null)
        {
            ingestionOptions?.ReportProgress("Nothing new to Ingest so skipping");
            return;
        }

        List<CSharpChunk> codeEntities = [];

        foreach (DataProviders.Models.FileContent file in files)
        {
            string content = file.GetContentAsUtf8String();
            int numberOfLine = content.Split(["\n"], StringSplitOptions.RemoveEmptyEntries).Length;
            if (IgnoreFileIfMoreThanThisNumberOfLines.HasValue && numberOfLine > IgnoreFileIfMoreThanThisNumberOfLines)
            {
                continue;
            }

            List<CSharpChunk> entitiesForFile = _chunker.GetChunks(content, ChunkerOptions);
            foreach (CSharpChunk codeEntity in entitiesForFile)
            {
                codeEntity.SourcePath = file.PathWithoutRoot;
            }

            codeEntities.AddRange(entitiesForFile);
        }

        ingestionOptions?.ReportProgress($"{files.Length} Files was transformed into {codeEntities.Count} Code Entities for Vector Import. Preparing Embedding step...");

        Func<CSharpChunk, string>? contentFormatBuilder = ContentFormatBuilder;
        if (contentFormatBuilder == null)
        {
            contentFormatBuilder = chunk =>
            {
                StringBuilder contentBuilder = new();
                string parentDetails = string.Empty;
                if (!string.IsNullOrWhiteSpace(chunk.Parent))
                {
                    parentDetails = $" parentKind=\"{chunk.ParentKindAsString}\" parent=\"{chunk.Parent}\"";
                }

                contentBuilder.AppendLine($"<code name=\"{chunk.Name}\" kind=\"{chunk.KindAsString}\" namespace=\"{chunk.Namespace}\"{parentDetails}>");
                contentBuilder.AppendLine(chunk.XmlSummary + " " + chunk.Value);
                if (chunk.Dependencies.Count > 0)
                {
                    contentBuilder.AppendLine("<dependencies>");
                    contentBuilder.AppendLine(string.Join(Environment.NewLine, chunk.Dependencies.Select(x => "- " + x)));
                    contentBuilder.AppendLine("</dependencies>");
                }

                if (chunk.References is { Count: > 0 })
                {
                    contentBuilder.AppendLine("<used_by>");
                    contentBuilder.AppendLine(string.Join(Environment.NewLine, chunk.References.Select(x => "- " + x.Path)));
                    contentBuilder.AppendLine("</used_by>");
                }

                contentBuilder.AppendLine("</code>");
                return contentBuilder.ToString();
            };

            //Creating References
            foreach (CSharpChunk codeEntity in codeEntities)
            {
                switch (codeEntity.Kind)
                {
                    case CSharpChunkKind.Enum:
                    case CSharpChunkKind.Interface:
                    case CSharpChunkKind.Constructor:
                    case CSharpChunkKind.Class:
                    case CSharpChunkKind.Struct:
                    case CSharpChunkKind.Record:
                        codeEntity.References = codeEntities.Where(x => x != codeEntity && x.Dependencies.Any(y => y == codeEntity.Name)).ToList();
                        break;
                }
            }

            IEnumerable<VectorEntity> vectorEntities = codeEntities.Select(x => new VectorEntity
            {
                Id = Guid.NewGuid().ToString(),
                SourceId = Id.Value,
                ContentId = null,
                SourceCollectionId = CollectionId.Value,
                SourceKind = DataSourceKinds.CSharp,
                SourcePath = x.SourcePath,
                ContentKind = x.KindAsString,
                ContentParent = x.Parent,
                ContentParentKind = x.ParentKindAsString,
                ContentName = x.Name,
                ContentNamespace = x.Namespace,
                ContentDependencies = x.Dependencies.Count == 0 ? null : string.Join(";", x.Dependencies),
                ContentReferences = x.References is { Count: 0 } ? null : string.Join(";", x.References?.Select(y => y.Path) ?? []),
                ContentDescription = x.XmlSummary,
                Content = contentFormatBuilder.Invoke(x),
            });

            await _vectorStoreCommand.SyncAsync(this, vectorEntities, ingestionOptions?.OnProgressNotification, cancellationToken);
            ingestionOptions?.ReportProgress("Done");
        }
    }
}
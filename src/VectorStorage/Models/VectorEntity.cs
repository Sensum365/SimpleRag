using JetBrains.Annotations;
using Microsoft.Extensions.VectorData;

namespace SimpleRag.VectorStorage.Models;

public class VectorEntity
{
    [VectorStoreKey]
    public required string Id { get; set; }

    [VectorStoreData]
    public required string Content { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public required string SourceId { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public required string SourceKind { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public required string SourceCollectionId { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public required string? ContentKind { get; init; }

    [VectorStoreData]
    public required string? ContentId { get; init; }

    [VectorStoreData]
    public required string? ContentParent { get; init; }

    [VectorStoreData(IsIndexed = true)]
    public required string? ContentParentKind { get; init; }

    [VectorStoreData]
    public required string? ContentName { get; set; }

    [VectorStoreData]
    public required string? ContentDependencies { get; set; }

    [VectorStoreData]
    public required string? ContentDescription { get; set; }

    [VectorStoreData]
    public required string? ContentReferences { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public required string? ContentNamespace { get; init; }

    [VectorStoreData]
    public required string SourcePath { get; init; }

    [VectorStoreVector(1536)]
    [UsedImplicitly]
    public string Vector => Content;

    public string GetContentCompareKey()
    {
        string contentCompareKey = ContentId + SourceKind + ContentKind + ContentName + ContentParent + ContentParentKind + ContentNamespace + ContentDependencies + ContentDescription + ContentReferences + Content;
        return contentCompareKey;
    }

    public string GetAsString()
    {
        return $"<search_result citation=\"todo\">{Content}</search_result>"; //todo: Citation url support
    }
}
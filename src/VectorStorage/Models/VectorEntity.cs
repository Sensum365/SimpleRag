using JetBrains.Annotations;
using Microsoft.Extensions.VectorData;

namespace SimpleRag.VectorStorage.Models;

/// <summary>
/// Represents a single record stored in the vector store.
/// </summary>
public class VectorEntity
{
    [VectorStoreKey]
    /// <summary>Gets or sets the unique identifier.</summary>
    public required string Id { get; set; }

    [VectorStoreData]
    /// <summary>Gets or sets the textual content.</summary>
    public required string Content { get; set; }

    [VectorStoreData(IsIndexed = true)]
    /// <summary>Gets or sets the identifier of the source.</summary>
    public required string SourceId { get; set; }

    [VectorStoreData(IsIndexed = true)]
    /// <summary>Gets or sets the kind of the source.</summary>
    public required string SourceKind { get; set; }

    [VectorStoreData(IsIndexed = true)]
    /// <summary>Gets or sets the collection identifier.</summary>
    public required string SourceCollectionId { get; set; }

    [VectorStoreData(IsIndexed = true)]
    /// <summary>Gets or sets the kind of the content.</summary>
    public required string? ContentKind { get; init; }

    [VectorStoreData]
    /// <summary>Gets or sets the content identifier.</summary>
    public required string? ContentId { get; init; }

    [VectorStoreData]
    /// <summary>Gets or sets the parent item of the content.</summary>
    public required string? ContentParent { get; init; }

    [VectorStoreData(IsIndexed = true)]
    /// <summary>Gets or sets the kind of the parent item.</summary>
    public required string? ContentParentKind { get; init; }

    [VectorStoreData]
    /// <summary>Gets or sets the name of the content.</summary>
    public required string? ContentName { get; set; }

    [VectorStoreData]
    /// <summary>Gets or sets content dependencies.</summary>
    public required string? ContentDependencies { get; set; }

    [VectorStoreData]
    /// <summary>Gets or sets the content description.</summary>
    public required string? ContentDescription { get; set; }

    [VectorStoreData]
    /// <summary>Gets or sets references related to the content.</summary>
    public required string? ContentReferences { get; set; }

    [VectorStoreData(IsIndexed = true)]
    /// <summary>Gets or sets the namespace of the content.</summary>
    public required string? ContentNamespace { get; init; }

    [VectorStoreData]
    /// <summary>Gets or sets the original file path of the source.</summary>
    public required string SourcePath { get; init; }

    [VectorStoreVector(1536)]
    [UsedImplicitly]
    /// <summary>Gets the vector embedding for the content.</summary>
    public string Vector => Content;

    /// <summary>
    /// Generates a unique key for comparing content.
    /// </summary>
    public string GetContentCompareKey()
    {
        string contentCompareKey = ContentId + SourceKind + ContentKind + ContentName + ContentParent + ContentParentKind + ContentNamespace + ContentDependencies + ContentDescription + ContentReferences + Content;
        return contentCompareKey;
    }

    /// <summary>
    /// Gets the entity as an XML fragment.
    /// </summary>
    public string GetAsString()
    {
        return $"<search_result citation=\"todo\">{Content}</search_result>"; //todo: Citation url support
    }
}
using JetBrains.Annotations;
using Microsoft.Extensions.VectorData;

namespace SimpleRag.VectorStorage.Models;

/// <summary>
/// Represents a single record stored in the vector store.
/// </summary>
[PublicAPI]
public class VectorEntity
{
    /// <summary>Gets or sets the unique identifier.</summary>
    [VectorStoreKey]
    public required string Id { get; set; }

    /// <summary>Gets or sets the textual content.</summary>
    [VectorStoreData]
    public required string Content { get; set; }

    /// <summary>Gets or sets the identifier of the source.</summary>
    [VectorStoreData(IsIndexed = true)]
    public required string SourceId { get; set; }

    /// <summary>Gets or sets the kind of the source.</summary>
    [VectorStoreData(IsIndexed = true)]
    public required string SourceKind { get; set; }

    /// <summary>Gets or sets the collection identifier.</summary>
    [VectorStoreData(IsIndexed = true)]
    public required string SourceCollectionId { get; set; }

    /// <summary>Gets or sets the kind of the content.</summary>
    [VectorStoreData(IsIndexed = true)]
    public required string? ContentKind { get; init; }

    /// <summary>Gets or sets the content identifier.</summary>
    [VectorStoreData]
    public required string? ContentId { get; init; }

    /// <summary>Gets or sets the parent item of the content.</summary>
    [VectorStoreData]
    public required string? ContentParent { get; init; }

    /// <summary>Gets or sets the kind of the parent item.</summary>
    [VectorStoreData(IsIndexed = true)]
    public required string? ContentParentKind { get; init; }

    /// <summary>Gets or sets the name of the content.</summary>
    [VectorStoreData]
    public required string? ContentName { get; set; }

    /// <summary>Gets or sets content dependencies.</summary>
    [VectorStoreData]
    public required string? ContentDependencies { get; set; }

    /// <summary>Gets or sets the content description.</summary>
    [VectorStoreData]
    public required string? ContentDescription { get; set; }

    /// <summary>Gets or sets references related to the content.</summary>
    [VectorStoreData]
    public required string? ContentReferences { get; set; }

    /// <summary>Gets or sets the namespace of the content.</summary>
    [VectorStoreData(IsIndexed = true)]
    public required string? ContentNamespace { get; init; }

    /// <summary>Gets or sets the original file path of the source.</summary>
    [VectorStoreData]
    public required string SourcePath { get; init; }

    /// <summary>Gets the vector embedding for the content.</summary>
    [VectorStoreVector(1536)]
    [UsedImplicitly]
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
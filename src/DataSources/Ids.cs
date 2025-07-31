using SimpleRag.VectorStorage.Models;

namespace SimpleRag.DataSources;

/// <summary>
/// Represent a CollectionId
/// </summary>
/// <param name="Value">The string-based value</param>
public readonly record struct CollectionId(string Value);

/// <summary>
/// Represent a SourceId
/// </summary>
/// <param name="Value">The string-based value</param>
public readonly record struct SourceId(string Value);

/// <summary>
/// Represent a Data collection
/// </summary>
public record Collection
{
    /// <summary>
    /// Id of the collection
    /// </summary>
    public required CollectionId Id { get; set; }

    /// <summary>
    /// Sources in the collection
    /// </summary>
    public required Source[] Sources { get; set; }
}

/// <summary>
/// Represent
/// </summary>
public record Source
{
    /// <summary>
    /// Id of the source
    /// </summary>
    public required SourceId Id { get; set; }

    /// <summary>
    /// The Vector Entities in the Vector Store
    /// </summary>
    public required VectorEntity[] VectorEntities { get; set; }
}
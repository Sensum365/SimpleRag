using JetBrains.Annotations;
using SimpleRag.DataSources;

namespace SimpleRag;

/// <summary>
/// Options controlling a search request.
/// </summary>
[PublicAPI]
public class SearchOptions
{
    /// <summary>
    /// Gets or sets the query to search for.
    /// </summary>
    public required string SearchQuery { get; set; }

    /// <summary>
    /// Gets or sets the number of records to return.
    /// </summary>
    public required int NumberOfRecordsBack { get; set; }

    /// <summary>
    /// Id of the collection
    /// </summary>
    public required CollectionId CollectionId { get; set; }

    /// <summary>
    /// Id of the source (optional)
    /// </summary>
    public SourceId? SourceId { get; set; }

    /// <summary>
    /// Kind option the source (optional)
    /// </summary>
    public string? SourceKind { get; set; }
}
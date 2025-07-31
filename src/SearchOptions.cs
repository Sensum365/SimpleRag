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

    /// <summary>
    /// The Threshold Score to return (Not called min/max as some vector stores see low numbers as most similar while others see high numbers as most similar. The system will check that on the fly and use the indicated score as min/max)
    /// </summary>
    public double? ThresholdSimilarityScoreToReturn { get; set; }

    /// <summary>
    /// How Search should use caching (or leave null to not use any caching)
    /// </summary>
    public SearchCachingStrategy? SearchCachingStrategy { get; set; }
}
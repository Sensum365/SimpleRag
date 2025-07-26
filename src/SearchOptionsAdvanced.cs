using System.Linq.Expressions;
using JetBrains.Annotations;
using SimpleRag.VectorStorage.Models;

namespace SimpleRag;

/// <summary>
/// Options controlling a search request.
/// </summary>
[PublicAPI]
public class SearchOptionsAdvanced
{
    /// <summary>Gets or sets the query to search for.</summary>
    public required string SearchQuery { get; set; }

    /// <summary>Gets or sets the number of records to return.</summary>
    public required int NumberOfRecordsBack { get; set; }

    /// <summary>Gets or sets an optional filter for the search.</summary>
    public Expression<Func<VectorEntity, bool>>? Filter { get; set; }

    /// <summary>
    /// The Threshold Score to return (Not called min/max as some vector stores see low numbers as most similar while others see high numbers as most similar. The system will check that on the fly and use the indicated score as min/max)
    /// </summary>
    public double? ThresholdSimilarityScoreToReturn { get; set; }
}
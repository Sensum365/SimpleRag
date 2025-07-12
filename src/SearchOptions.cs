using JetBrains.Annotations;
using SimpleRag.DataSources.CSharp.Models;
using SimpleRag.VectorStorage.Models;
using System.Linq.Expressions;

namespace SimpleRag;

/// <summary>
/// Options controlling a search request.
/// </summary>
[PublicAPI]
public class SearchOptions
{
    /// <summary>Gets or sets the query to search for.</summary>
    public required string SearchQuery { get; set; }

    /// <summary>Gets or sets the number of records to return.</summary>
    public required int NumberOfRecordsBack { get; set; }

    /// <summary>Gets or sets an optional filter for the search.</summary>
    public Expression<Func<VectorEntity, bool>>? Filter { get; set; }
}
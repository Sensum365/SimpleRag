using JetBrains.Annotations;
using Microsoft.Extensions.VectorData;
using System.Text;

namespace SimpleRag.VectorStorage.Models;

/// <summary>
/// Represents the result of a vector search.
/// </summary>
[PublicAPI]
public class SearchResult
{
    /// <summary>Gets or sets the returned entities.</summary>
    public required VectorSearchResult<VectorEntity>[] Entities { get; init; }

    /// <summary>
    /// Formats the result as an XML fragment.
    /// <param name="citationBuilder">Builder of the desired citation</param>
    /// </summary>
    public string GetAsStringResult(Func<VectorEntity, string>? citationBuilder = null)
    {
        StringBuilder sb = new();
        sb.AppendLine("<search_results>");
        foreach (VectorSearchResult<VectorEntity> entity in Entities)
        {
            sb.AppendLine(entity.Record.GetAsString(citationBuilder));
        }

        sb.AppendLine("</search_results>");
        return sb.ToString();
    }
}
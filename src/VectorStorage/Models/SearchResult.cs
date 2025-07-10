using System.Text;
using Microsoft.Extensions.VectorData;

namespace SimpleRag.VectorStorage.Models;

/// <summary>
/// Represents the result of a vector search.
/// </summary>
public class SearchResult
{
    /// <summary>Gets or sets the returned entities.</summary>
    public required VectorSearchResult<VectorEntity>[] Entities { get; set; }

    /// <summary>
    /// Formats the result as an XML fragment.
    /// </summary>
    public string GetAsStringResult()
    {
        StringBuilder sb = new();
        sb.AppendLine("<search_results>");
        foreach (var entity in Entities)
        {
            sb.AppendLine(entity.Record.GetAsString());
        }

        sb.AppendLine("</search_results>");
        return sb.ToString();
    }
}
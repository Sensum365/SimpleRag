using System.Text;
using Microsoft.Extensions.VectorData;

namespace SimpleRag.VectorStorage.Models;

public class SearchResult
{
    public required VectorSearchResult<VectorEntity>[] Entities { get; set; }

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
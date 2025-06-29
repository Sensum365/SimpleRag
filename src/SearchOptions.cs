using System.Linq.Expressions;
using SimpleRag.VectorStorage.Models;

namespace SimpleRag;

public class SearchOptions
{
    public required string SearchQuery { get; set; }
    public required int NumberOfRecordsBack { get; set; }
    public Expression<Func<VectorEntity, bool>>? Filter { get; set; }
}
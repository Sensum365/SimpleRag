using JetBrains.Annotations;
using SimpleRag.Models;

namespace SimpleRag;

/// <summary>
/// Options used during ingestion of data sources.
/// </summary>
[PublicAPI]
public class IngestionOptions
{
    /// <summary>
    /// Action to execute on progress
    /// </summary>
    public Action<ProgressNotification>? OnProgressNotification { get; set; }
}
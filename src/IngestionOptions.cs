using JetBrains.Annotations;

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
    public Action<Notification>? OnProgressNotification { get; set; }

    internal void ReportProgress(string message, int? current = null, int? total = null, string? details = null)
    {
        OnProgressNotification?.Invoke(new Notification(DateTimeOffset.UtcNow, message, current, total, details));
    }
}
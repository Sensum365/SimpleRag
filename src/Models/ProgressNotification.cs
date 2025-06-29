namespace SimpleRag.Models;

public record ProgressNotification(DateTimeOffset Timestamp, string Message, int Current = 0, int Total = 0, string? Details = null)
{
    public object? Arguments { get; init; }

    public string GetFormattedMessageWithDetails()
    {
        if (Current != 0 && Total != 0)
        {
            return !string.IsNullOrWhiteSpace(Details) ? $"{Message} ({Current}/{Total}) [{Details}]" : $"{Message} ({Current}/{Total})";
        }
        else
        {
            return !string.IsNullOrWhiteSpace(Details) ? $"{Message} [{Details}]" : $"{Message}";
        }
    }

    public string GetFormattedMessage()
    {
        if (Current != 0 && Total != 0)
        {
            return $"{Message} ({Current}/{Total})";
        }
        else
        {
            return $"{Message}";
        }
    }
}
using JetBrains.Annotations;

namespace SimpleRag.Models;

/// <summary>
/// Represents a progress notification.
/// </summary>
/// <param name="Timestamp">The time the notification was created.</param>
/// <param name="Message">The message describing the progress.</param>
/// <param name="Current">The current progress value.</param>
/// <param name="Total">The total progress value.</param>
/// <param name="Details">Optional additional details.</param>
[PublicAPI]
public record ProgressNotification(DateTimeOffset Timestamp, string Message, int Current = 0, int Total = 0, string? Details = null)
{
    /// <summary>Gets or sets optional additional arguments.</summary>
    public object? Arguments { get; init; }

    /// <summary>
    /// Gets the formatted message including details.
    /// </summary>
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

    /// <summary>
    /// Gets the formatted message.
    /// </summary>
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

    /// <summary>
    /// Create a simple Notification
    /// </summary>
    /// <param name="message">The Message</param>
    /// <param name="current">Current</param>
    /// <param name="total">Total</param>
    /// <param name="details">Details</param>
    /// <returns>Notification</returns>
    public static ProgressNotification Create(string message, int current = 0, int total = 0, string? details = null)
    {
        return new ProgressNotification(DateTimeOffset.UtcNow, message, current, total, details);
    }
}
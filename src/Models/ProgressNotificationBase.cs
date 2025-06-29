namespace SimpleRag.Models;

public class ProgressNotificationBase
{
    public event Action<ProgressNotification>? NotifyProgress;

    protected void OnNotifyProgress(string message, int current = 0, int total = 0, string? details = null)
    {
        NotifyProgress?.Invoke(new ProgressNotification(DateTimeOffset.UtcNow, message, current, total, details));
    }

    public void OnNotifyProgress(ProgressNotification notification)
    {
        NotifyProgress?.Invoke(notification);
    }
}
using Polly;
using Polly.Retry;

namespace SimpleRag.Helpers;

internal static class RetryHelper
{
    public static async Task ExecuteWithRetryAsync(Func<Task> func, int retries, TimeSpan waitTime)
    {
        PolicyBuilder assertException = Policy.Handle<Exception>();
        AsyncRetryPolicy retryPolicy = assertException.WaitAndRetryAsync(retries, _ => waitTime);
        await retryPolicy.ExecuteAsync(func);
    }
}
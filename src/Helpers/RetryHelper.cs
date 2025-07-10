using Polly;
using Polly.Retry;

namespace SimpleRag.Helpers;

internal static class RetryHelper
{
    /// <summary>
    /// Executes the specified function with retry logic.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="retries">The number of retries to attempt.</param>
    /// <param name="waitTime">The delay between retries.</param>
    public static async Task ExecuteWithRetryAsync(Func<Task> func, int retries, TimeSpan waitTime)
    {
        PolicyBuilder assertException = Policy.Handle<Exception>();
        AsyncRetryPolicy retryPolicy = assertException.WaitAndRetryAsync(retries, _ => waitTime);
        await retryPolicy.ExecuteAsync(func);
    }
}
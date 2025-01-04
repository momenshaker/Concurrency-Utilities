namespace Concurrency_Utilities.Interfaces;

public interface ITaskSchedulerWithRetry
{
    /// <summary>
    /// Executes the provided task with retry logic and exponential backoff.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the task.</typeparam>
    /// <param name="taskFunc">The task to execute, which returns a result of type T.</param>
    /// <param name="maxRetryAttempts">Maximum number of retry attempts (default is 3).</param>
    /// <param name="delayBetweenRetries">The delay between retries (default is 2 seconds).</param>
    /// <param name="backoffFactor">The factor by which the delay increases after each retry (default is 2.0 for exponential backoff).</param>
    /// <param name="maxDelay">The maximum delay between retries (default is no maximum).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <param name="onRetry">Optional callback to execute on each retry, receiving retry count and exception.</param>
    /// <param name="shouldRetry">Optional condition to determine whether a retry should occur based on the exception.</param>
    /// <returns>Returns the result of type T from the task after successful execution.</returns>
    Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> taskFunc,
        int maxRetryAttempts = 3,
        TimeSpan? delayBetweenRetries = null,
        double backoffFactor = 2.0,
        TimeSpan? maxDelay = null,
        CancellationToken cancellationToken = default,
        Action<int, Exception>? onRetry = null,
        Func<Exception, bool>? shouldRetry = null);

    /// <summary>
    /// Executes the provided task with a timeout, cancelling if the task takes too long.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the task.</typeparam>
    /// <param name="taskFunc">The task to execute, which returns a result of type T.</param>
    /// <param name="timeout">The timeout duration.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>Returns the result of type T from the task after successful execution within the timeout period.</returns>
    Task<T> ExecuteWithTimeoutAsync<T>(
        Func<Task<T>> taskFunc,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the task with both retry logic and timeout. The task will be retried with backoff and will time out if it takes too long.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the task.</typeparam>
    /// <param name="taskFunc">The task to execute, which returns a result of type T.</param>
    /// <param name="maxRetryAttempts">Maximum number of retry attempts (default is 3).</param>
    /// <param name="delayBetweenRetries">The delay between retries (default is 2 seconds).</param>
    /// <param name="backoffFactor">The factor by which the delay increases after each retry (default is 2.0 for exponential backoff).</param>
    /// <param name="maxDelay">The maximum delay between retries (default is no maximum).</param>
    /// <param name="timeout">The timeout duration for the entire retry operation.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <param name="onRetry">Optional callback to execute on each retry, receiving retry count and exception.</param>
    /// <param name="shouldRetry">Optional condition to determine whether a retry should occur based on the exception.</param>
    /// <returns>Returns the result of type T from the task after successful execution.</returns>
    Task<T> ExecuteWithRetryAndTimeoutAsync<T>(
        Func<Task<T>> taskFunc,
        int maxRetryAttempts = 3,
        TimeSpan? delayBetweenRetries = null,
        double backoffFactor = 2.0,
        TimeSpan? maxDelay = null,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default,
        Action<int, Exception>? onRetry = null,
        Func<Exception, bool>? shouldRetry = null);

    /// <summary>
    /// Executes the provided task with a Circuit Breaker mechanism to avoid retrying too many times.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the task.</typeparam>
    /// <param name="taskFunc">The task to execute, which returns a result of type T.</param>
    /// <param name="maxRetryAttempts">Maximum number of retry attempts (default is 3).</param>
    /// <param name="delayBetweenRetries">The delay between retries (default is 2 seconds).</param>
    /// <param name="backoffFactor">The factor by which the delay increases after each retry (default is 2.0 for exponential backoff).</param>
    /// <param name="maxDelay">The maximum delay between retries (default is no maximum).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <param name="onRetry">Optional callback to execute on each retry, receiving retry count and exception.</param>
    /// <param name="shouldRetry">Optional condition to determine whether a retry should occur based on the exception.</param>
    /// <returns>Returns the result of type T from the task after successful execution.</returns>
    Task<T> ExecuteWithCircuitBreakerAsync<T>(
        Func<Task<T>> taskFunc,
        int maxRetryAttempts = 3,
        TimeSpan? delayBetweenRetries = null,
        double backoffFactor = 2.0,
        TimeSpan? maxDelay = null,
        CancellationToken cancellationToken = default,
        Action<int, Exception>? onRetry = null,
        Func<Exception, bool>? shouldRetry = null);

    /// <summary>
    /// Executes the provided task using an adaptive backoff strategy (Fibonacci backoff).
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the task.</typeparam>
    /// <param name="taskFunc">The task to execute, which returns a result of type T.</param>
    /// <param name="maxRetryAttempts">Maximum number of retry attempts (default is 3).</param>
    /// <param name="delayBetweenRetries">The delay between retries (default is 2 seconds).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <param name="onRetry">Optional callback to execute on each retry, receiving retry count and exception.</param>
    /// <param name="shouldRetry">Optional condition to determine whether a retry should occur based on the exception.</param>
    /// <returns>Returns the result of type T from the task after successful execution.</returns>
    Task<T> ExecuteWithAdaptiveBackoffAsync<T>(
        Func<Task<T>> taskFunc,
        int maxRetryAttempts = 3,
        TimeSpan? delayBetweenRetries = null,
        CancellationToken cancellationToken = default,
        Action<int, Exception>? onRetry = null,
        Func<Exception, bool>? shouldRetry = null);
}

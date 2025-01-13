namespace Concurrency_Utilities.Interfaces;

public interface ITaskSchedulerWithRetry
{
    /// <summary>
    /// Executes the provided task with retry logic, exponential backoff, and jitter.
    /// </summary>
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
    /// Executes the provided task with retry logic and timeout.
    /// </summary>
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
    /// Executes the task with a timeout.
    /// </summary>
    Task<T> ExecuteWithTimeoutAsync<T>(
        Func<Task<T>> taskFunc,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the provided task with a Circuit Breaker mechanism.
    /// </summary>
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
    Task<T> ExecuteWithAdaptiveBackoffAsync<T>(
        Func<Task<T>> taskFunc,
        int maxRetryAttempts = 3,
        TimeSpan? delayBetweenRetries = null,
        CancellationToken cancellationToken = default,
        Action<int, Exception>? onRetry = null,
        Func<Exception, bool>? shouldRetry = null);

    /// <summary>
    /// Executes the task with retry, timeout, and circuit breaker mechanisms together.
    /// </summary>
    Task<T> ExecuteWithRetryTimeoutAndCircuitBreakerAsync<T>(
        Func<Task<T>> taskFunc,
        int maxRetryAttempts = 3,
        TimeSpan? delayBetweenRetries = null,
        double backoffFactor = 2.0,
        TimeSpan? maxDelay = null,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default,
        Action<int, Exception>? onRetry = null,
        Func<Exception, bool>? shouldRetry = null);
}

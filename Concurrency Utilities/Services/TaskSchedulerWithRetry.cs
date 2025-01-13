using Concurrency_Utilities.Extensions;
using Concurrency_Utilities.Interfaces;

public class TaskSchedulerWithRetry : ITaskSchedulerWithRetry
{
    private int _failureCount = 0;
    private const int CircuitBreakerThreshold = 5;
    private DateTime _lastFailureTime;
    private readonly Random _random = new Random();

    /// <summary>
    /// Executes the provided task with retry logic, exponential backoff, and jitter.
    /// </summary>
    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> taskFunc,
        int maxRetryAttempts = 3,
        TimeSpan? delayBetweenRetries = null,
        double backoffFactor = 2.0,
        TimeSpan? maxDelay = null,
        CancellationToken cancellationToken = default,
        Action<int, Exception>? onRetry = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        delayBetweenRetries ??= TimeSpan.FromSeconds(2);
        maxDelay ??= TimeSpan.FromMinutes(1);
        int retryAttempts = 0;
        TimeSpan currentDelay = delayBetweenRetries.Value;

        while (retryAttempts < maxRetryAttempts)
        {
            try
            {
                return await taskFunc();
            }
            catch (Exception ex) when (retryAttempts < maxRetryAttempts - 1 && (shouldRetry?.Invoke(ex) ?? true))
            {
                retryAttempts++;
                onRetry?.Invoke(retryAttempts, ex);

                // Exponential backoff with jitter (random delay)
                var jitter = TimeSpan.FromMilliseconds(_random.Next(0, 500));  // Adding jitter of up to 500ms
                var nextDelay = Math.Min(currentDelay.TotalMilliseconds * backoffFactor + jitter.TotalMilliseconds, maxDelay.Value.TotalMilliseconds);
                currentDelay = TimeSpan.FromMilliseconds(nextDelay);

                // Wait with the calculated delay before retrying
                await Task.Delay(currentDelay, cancellationToken);
            }
        }

        throw new Exception("Maximum retry attempts reached.");
    }

    /// <summary>
    /// Executes the provided task with a timeout and retry logic.
    /// </summary>
    public async Task<T> ExecuteWithRetryAndTimeoutAsync<T>(
        Func<Task<T>> taskFunc,
        int maxRetryAttempts = 3,
        TimeSpan? delayBetweenRetries = null,
        double backoffFactor = 2.0,
        TimeSpan? maxDelay = null,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default,
        Action<int, Exception>? onRetry = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        var retryTask = ExecuteWithRetryAsync(
            taskFunc,
            maxRetryAttempts,
            delayBetweenRetries,
            backoffFactor,
            maxDelay,
            cancellationToken,
            onRetry,
            shouldRetry);

        return await ExecuteWithTimeoutAsync(() => retryTask, timeout, cancellationToken);
    }

    /// <summary>
    /// Executes the provided task with a timeout, cancelling if the task takes too long.
    /// </summary>
    public async Task<T> ExecuteWithTimeoutAsync<T>(
        Func<Task<T>> taskFunc,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using (var cts = new CancellationTokenSource(timeout))
        {
            // Combine the timeout cancellation and user cancellation token.
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

            try
            {
                return await taskFunc().WithCancellation(linkedCts.Token);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("The operation timed out.");
            }
        }
    }

    /// <summary>
    /// Executes the provided task with a Circuit Breaker mechanism.
    /// </summary>
    public async Task<T> ExecuteWithCircuitBreakerAsync<T>(
        Func<Task<T>> taskFunc,
        int maxRetryAttempts = 3,
        TimeSpan? delayBetweenRetries = null,
        double backoffFactor = 2.0,
        TimeSpan? maxDelay = null,
        CancellationToken cancellationToken = default,
        Action<int, Exception>? onRetry = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        if (_failureCount >= CircuitBreakerThreshold && DateTime.UtcNow - _lastFailureTime < TimeSpan.FromMinutes(1))
        {
            throw new InvalidOperationException("Circuit breaker is open. Retry operation is not allowed.");
        }

        try
        {
            return await ExecuteWithRetryAsync(
                taskFunc,
                maxRetryAttempts,
                delayBetweenRetries,
                backoffFactor,
                maxDelay,
                cancellationToken,
                onRetry,
                shouldRetry);
        }
        catch (Exception ex)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            throw new Exception("The task failed after multiple retries.", ex);
        }
    }

    /// <summary>
    /// Executes the provided task using an adaptive backoff strategy (Fibonacci backoff).
    /// </summary>
    public async Task<T> ExecuteWithAdaptiveBackoffAsync<T>(
        Func<Task<T>> taskFunc,
        int maxRetryAttempts = 3,
        TimeSpan? delayBetweenRetries = null,
        CancellationToken cancellationToken = default,
        Action<int, Exception>? onRetry = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        delayBetweenRetries ??= TimeSpan.FromSeconds(2);
        var retryDelays = new List<TimeSpan> { delayBetweenRetries.Value };

        // Fibonacci-like backoff
        for (int i = 1; i < maxRetryAttempts; i++)
        {
            retryDelays.Add(retryDelays[i - 1] + retryDelays[Math.Max(0, i - 2)]);
        }

        int retryAttempts = 0;
        while (retryAttempts < maxRetryAttempts)
        {
            try
            {
                return await taskFunc();
            }
            catch (Exception ex) when (retryAttempts < maxRetryAttempts - 1 && (shouldRetry?.Invoke(ex) ?? true))
            {
                retryAttempts++;
                onRetry?.Invoke(retryAttempts, ex);

                // Wait before retrying based on Fibonacci backoff
                await Task.Delay(retryDelays[retryAttempts], cancellationToken);
            }
        }

        throw new Exception("Maximum retry attempts reached.");
    }

    /// <summary>
    /// Executes the task with retry, timeout, and circuit breaker mechanisms together.
    /// </summary>
    public async Task<T> ExecuteWithRetryTimeoutAndCircuitBreakerAsync<T>(
        Func<Task<T>> taskFunc,
        int maxRetryAttempts = 3,
        TimeSpan? delayBetweenRetries = null,
        double backoffFactor = 2.0,
        TimeSpan? maxDelay = null,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default,
        Action<int, Exception>? onRetry = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        // First, retry logic is executed, then the timeout and circuit breaker are applied
        var result = await ExecuteWithCircuitBreakerAsync(
            taskFunc,
            maxRetryAttempts,
            delayBetweenRetries,
            backoffFactor,
            maxDelay,
            cancellationToken,
            onRetry,
            shouldRetry);

        return await ExecuteWithTimeoutAsync(() => Task.FromResult(result), timeout, cancellationToken);
    }
}

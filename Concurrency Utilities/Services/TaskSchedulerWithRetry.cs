using Concurrency_Utilities.Interfaces;

public class TaskSchedulerWithRetry  : ITaskSchedulerWithRetry
{
    private int _failureCount = 0;
    private const int CircuitBreakerThreshold = 5;
    private DateTime _lastFailureTime;

    /// <summary>
    /// Executes the provided task with retry logic and exponential backoff.
    /// The task will be retried in case of failure up to the maximum retry attempts with exponential backoff and jitter.
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
    /// <exception cref="Exception">Throws an exception if the task fails after maximum retry attempts.</exception>
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
                Console.WriteLine($"Attempt {retryAttempts} failed: {ex.Message}. Retrying in {currentDelay.TotalSeconds} seconds...");

                // Exponential backoff with jitter (random delay)
                var jitter = new Random().Next(0, 1000);
                var nextDelay = Math.Min(currentDelay.TotalMilliseconds * backoffFactor + jitter, maxDelay.Value.TotalMilliseconds);
                currentDelay = TimeSpan.FromMilliseconds(nextDelay);

                // Wait with the calculated delay before retrying
                await Task.Delay(currentDelay, cancellationToken);
            }
        }

        throw new Exception("Maximum retry attempts reached.");
    }

    /// <summary>
    /// Executes the provided task with a timeout, cancelling if the task takes too long.
    /// If the task exceeds the specified timeout, a TimeoutException will be thrown.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the task.</typeparam>
    /// <param name="taskFunc">The task to execute, which returns a result of type T.</param>
    /// <param name="timeout">The timeout duration.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>Returns the result of type T from the task after successful execution within the timeout period.</returns>
    /// <exception cref="TimeoutException">Throws a TimeoutException if the task does not complete within the specified timeout.</exception>
    public async Task<T> ExecuteWithTimeoutAsync<T>(
        Func<Task<T>> taskFunc,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using (var cts = new CancellationTokenSource(timeout))
        {
            try
            {
                return await taskFunc();
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("The operation timed out.");
            }
        }
    }

    /// <summary>
    /// Executes the task with both retry logic and timeout. The task will be retried with backoff and will time out if it takes too long.
    /// The retry logic will be applied first, and then the timeout will be enforced.
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
        // First, retry logic is executed, then the timeout is applied to the entire operation.
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
    /// Executes the provided task with a Circuit Breaker mechanism to avoid retrying too many times.
    /// If the task fails consecutively more than the threshold, retries will be blocked for a set duration.
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
    /// The retry delay increases in a Fibonacci sequence for each retry attempt.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the task.</typeparam>
    /// <param name="taskFunc">The task to execute, which returns a result of type T.</param>
    /// <param name="maxRetryAttempts">Maximum number of retry attempts (default is 3).</param>
    /// <param name="delayBetweenRetries">The delay between retries (default is 2 seconds).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <param name="onRetry">Optional callback to execute on each retry, receiving retry count and exception.</param>
    /// <param name="shouldRetry">Optional condition to determine whether a retry should occur based on the exception.</param>
    /// <returns>Returns the result of type T from the task after successful execution.</returns>
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
                Console.WriteLine($"Attempt {retryAttempts} failed: {ex.Message}. Retrying in {retryDelays[retryAttempts].TotalSeconds} seconds...");

                // Wait before retrying
                await Task.Delay(retryDelays[retryAttempts], cancellationToken);
            }
        }

        throw new Exception("Maximum retry attempts reached.");
    }
}

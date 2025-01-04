namespace Concurrency_Utilities.Interfaces;

public interface IParallelBatchProcessor
{
    /// <summary>
    /// Processes a batch of items asynchronously with a configurable degree of parallelism.
    /// </summary>
    /// <typeparam name="T">The type of the items in the batch.</typeparam>
    /// <param name="items">The collection of items to process.</param>
    /// <param name="processItem">The function to process each item.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of tasks to run concurrently (default is 4).</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task ProcessBatchAsync<T>(
        IEnumerable<T> items,
        Func<T, Task> processItem,
        int maxDegreeOfParallelism = 4,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a batch of items using parallelism with a configurable degree of parallelism.
    /// </summary>
    /// <typeparam name="T">The type of the items in the batch.</typeparam>
    /// <param name="items">The collection of items to process.</param>
    /// <param name="processItem">The action to process each item.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of tasks to run concurrently (default is 4).</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    void ProcessBatchWithParallel<T>(
        IEnumerable<T> items,
        Action<T> processItem,
        int maxDegreeOfParallelism = 4,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a batch of items asynchronously with progress reporting.
    /// </summary>
    /// <typeparam name="T">The type of the items in the batch.</typeparam>
    /// <param name="items">The collection of items to process.</param>
    /// <param name="processItem">The function to process each item.</param>
    /// <param name="onProgress">The action to report progress.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task ProcessBatchWithProgressAsync<T>(
        IEnumerable<T> items,
        Func<T, Task> processItem,
        Action<int> onProgress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes batch processing with retry for failed items.
    /// </summary>
    /// <typeparam name="T">The type of the items in the batch.</typeparam>
    /// <param name="items">The collection of items to process.</param>
    /// <param name="processItem">The function to process each item.</param>
    /// <param name="maxRetryAttempts">The maximum number of retry attempts for failed items.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task ProcessBatchWithRetryAsync<T>(
        IEnumerable<T> items,
        Func<T, Task> processItem,
        int maxRetryAttempts = 3,
        CancellationToken cancellationToken = default);
}

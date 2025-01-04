using Concurrency_Utilities.Interfaces;
using System.Threading.Tasks.Dataflow;

public class ParallelBatchProcessor : IParallelBatchProcessor
{
    /// <summary>
    /// Processes a batch of items asynchronously, with configurable degree of parallelism.
    /// </summary>
    /// <typeparam name="T">The type of the items in the batch.</typeparam>
    /// <param name="items">The collection of items to process.</param>
    /// <param name="processItem">The function to process each item.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of tasks to run concurrently (default is 4).</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task ProcessBatchAsync<T>(
        IEnumerable<T> items,
        Func<T, Task> processItem,
        int maxDegreeOfParallelism = 4,
        CancellationToken cancellationToken = default)
    {
        var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);  // Semaphore to limit concurrency
        var tasks = items.Select(async item =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                await processItem(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing item: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Processes a batch of items using parallelism, with configurable degree of parallelism.
    /// </summary>
    /// <typeparam name="T">The type of the items in the batch.</typeparam>
    /// <param name="items">The collection of items to process.</param>
    /// <param name="processItem">The action to process each item.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of tasks to run concurrently (default is 4).</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    public void ProcessBatchWithParallel<T>(
        IEnumerable<T> items,
        Action<T> processItem,
        int maxDegreeOfParallelism = 4,
        CancellationToken cancellationToken = default)
    {
        var options = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        var block = new ActionBlock<T>(item =>
        {
            processItem(item);
        }, options);

        foreach (var item in items)
        {
            block.Post(item);
        }

        block.Complete();
        block.Completion.Wait();  // Wait for the processing to complete
    }

    /// <summary>
    /// Processes a batch of items asynchronously, with progress reporting.
    /// </summary>
    /// <typeparam name="T">The type of the items in the batch.</typeparam>
    /// <param name="items">The collection of items to process.</param>
    /// <param name="processItem">The function to process each item.</param>
    /// <param name="onProgress">The action to report progress.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task ProcessBatchWithProgressAsync<T>(
        IEnumerable<T> items,
        Func<T, Task> processItem,
        Action<int> onProgress,
        CancellationToken cancellationToken = default)
    {
        int totalItems = items.Count();
        int processedItems = 0;

        var tasks = items.Select(async item =>
        {
            await processItem(item);
            processedItems++;
            onProgress?.Invoke((int)((double)processedItems / totalItems * 100));  // Report progress
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Executes batch processing with retry for failed items.
    /// </summary>
    /// <typeparam name="T">The type of the items in the batch.</typeparam>
    /// <param name="items">The collection of items to process.</param>
    /// <param name="processItem">The function to process each item.</param>
    /// <param name="maxRetryAttempts">The maximum number of retry attempts for failed items.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    public async Task ProcessBatchWithRetryAsync<T>(
        IEnumerable<T> items,
        Func<T, Task> processItem,
        int maxRetryAttempts = 3,
        CancellationToken cancellationToken = default)
    {
        foreach (var item in items)
        {
            int attempt = 0;
            while (attempt < maxRetryAttempts)
            {
                try
                {
                    await processItem(item);
                    break;  // Break on success
                }
                catch (Exception ex)
                {
                    attempt++;
                    Console.WriteLine($"Error processing item: {ex.Message}. Retrying... (Attempt {attempt}/{maxRetryAttempts})");

                    if (attempt == maxRetryAttempts)
                    {
                        Console.WriteLine($"Max retry attempts reached for item: {item}. Skipping.");
                    }
                    await Task.Delay(1000, cancellationToken);  // Optional delay between retries
                }
            }
        }
    }
}

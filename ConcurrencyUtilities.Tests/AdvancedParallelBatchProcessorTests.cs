using Moq;
using Xunit;

public class AdvancedParallelBatchProcessorTests
{
    private readonly ParallelBatchProcessor _processor;

    public AdvancedParallelBatchProcessorTests()
    {
        _processor = new ParallelBatchProcessor();
    }

    // Test 1: ProcessBatchAsync - Successfully process all items
    [Fact]
    public async Task ProcessBatchAsync_ShouldProcessAllItemsSuccessfully()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4 };
        var processedItems = new List<int>();

        Func<int, Task> processItem = async (item) =>
        {
            processedItems.Add(item);
        };

        // Act
        await _processor.ProcessBatchAsync(items, processItem);

        // Assert
        Assert.Equal(items.Count, processedItems.Count);
        Assert.True(items.All(item => processedItems.Contains(item)));
    }

    // Test 2: ProcessBatchAsync - Cancellation should stop processing
    [Fact]
    public async Task ProcessBatchAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4 };
        var cancellationTokenSource = new CancellationTokenSource();
        var processedItems = new List<int>();

        Func<int, Task> processItem = async (item) =>
        {
            await Task.Delay(50);  // Simulate processing delay
            processedItems.Add(item);
        };

        // Act
        var task = _processor.ProcessBatchAsync(items, processItem, cancellationToken: cancellationTokenSource.Token);

        // Cancel after 100ms
        cancellationTokenSource.Cancel();

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        Assert.True(processedItems.Count < items.Count);  // Not all items should be processed
    }

    // Test 3: ProcessBatchWithParallel - Parallel processing with Action delegate
    [Fact]
    public void ProcessBatchWithParallel_ShouldProcessItemsInParallel()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4 };
        var processedItems = new List<int>();

        Action<int> processItem = (item) =>
        {
            processedItems.Add(item);
        };

        // Act
        _processor.ProcessBatchWithParallel(items, processItem, maxDegreeOfParallelism: 3);

        // Assert
        Assert.Equal(items.Count, processedItems.Count);
        Assert.True(items.All(item => processedItems.Contains(item)));
    }

    // Test 4: ProcessBatchWithProgressAsync - Report progress correctly
    [Fact]
    public async Task ProcessBatchWithProgressAsync_ShouldReportProgressCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4 };
        var progressList = new List<int>();

        Func<int, Task> processItem = async (item) =>
        {
            await Task.Delay(50);  // Simulate processing delay
        };

        Action<int> onProgress = progress =>
        {
            progressList.Add(progress);
        };

        // Act
        await _processor.ProcessBatchWithProgressAsync(items, processItem, onProgress);

        // Assert
        Assert.Equal(items.Count, progressList.Count);
        Assert.Equal(new List<int> { 25, 50, 75, 100 }, progressList);
    }

  
    // Test 5: ProcessBatchWithRetryAsync - Cancellation during retry should stop processing
    [Fact]
    public async Task ProcessBatchWithRetryAsync_ShouldRespectCancellationDuringRetry()
    {
        // Arrange
        var items = new List<int> { 1, 2 };
        var cancellationTokenSource = new CancellationTokenSource();
        var processedItems = new List<int>();

        Func<int, Task> processItem = async (item) =>
        {
            await Task.Delay(100);  // Simulate processing delay
            processedItems.Add(item);
            throw new Exception("Simulated failure");  // Always fail to trigger retry
        };

        // Act
        var task = _processor.ProcessBatchWithRetryAsync(items, processItem, maxRetryAttempts: 2, cancellationToken: cancellationTokenSource.Token);

        // Cancel after 150ms
        await Task.Delay(150);
        cancellationTokenSource.Cancel();

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        Assert.True(processedItems.Count < items.Count);  // Not all items should be processed
    }
}

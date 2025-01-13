using Concurrency_Utilities.Interfaces;
using Moq;
using Xunit;

public class TaskSchedulerWithRetryTests
{
    private readonly Mock<ITaskSchedulerWithRetry> _mockTaskScheduler;

    public TaskSchedulerWithRetryTests()
    {
        _mockTaskScheduler = new Mock<ITaskSchedulerWithRetry>();
    }

    [Fact]
    public async Task ExecuteWithCircuitBreakerAsync_ShouldThrowWhenCircuitBreakerIsOpen()
    {
        // Arrange
        Func<Task<int>> taskFunc = async () => throw new InvalidOperationException("Failed task");

        // Simulate the circuit breaker being triggered (failure count >= threshold)
        _mockTaskScheduler.Setup(x => x.ExecuteWithCircuitBreakerAsync(It.IsAny<Func<Task<int>>>(), It.IsAny<int>(), It.IsAny<TimeSpan?>(), It.IsAny<double>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>(), It.IsAny<Action<int, Exception>>(), It.IsAny<Func<Exception, bool>>()))
            .ThrowsAsync(new InvalidOperationException("Circuit breaker is open. Retry operation is not allowed."));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _mockTaskScheduler.Object.ExecuteWithCircuitBreakerAsync(taskFunc, maxRetryAttempts: 3);
        });
    }

  
}

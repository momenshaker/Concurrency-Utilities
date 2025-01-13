namespace Concurrency_Utilities.Extensions;

public static class TaskExtensions
{
    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var cancellationTask = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

        var completedTask = await Task.WhenAny(task, cancellationTask);
        return await task;
    }
}

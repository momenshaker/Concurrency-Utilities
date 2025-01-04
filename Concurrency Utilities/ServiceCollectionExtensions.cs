using Concurrency_Utilities.Interfaces;  // Ensure correct namespace for the interfaces
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the TaskSchedulerWithRetry service with dependency injection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddTaskSchedulerWithRetry(this IServiceCollection services)
    {
        // Register TaskSchedulerWithRetry as a singleton or transient service based on preference
        services.AddSingleton<ITaskSchedulerWithRetry, TaskSchedulerWithRetry>();

        return services;
    }

    /// <summary>
    /// Registers the ParallelBatchProcessor service with dependency injection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddParallelBatchProcessor(this IServiceCollection services)
    {
        // Register ParallelBatchProcessor as a singleton or transient service based on preference
        services.AddSingleton<IParallelBatchProcessor, ParallelBatchProcessor>();

        return services;
    }

    /// <summary>
    /// Registers the AdvancedConcurrentCache service with dependency injection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddAdvancedConcurrentCache(this IServiceCollection services)
    {
        // Register AdvancedConcurrentCache as a singleton or transient service based on preference
        services.AddSingleton<IAdvancedConcurrentCache, AdvancedConcurrentCache>();

        return services;
    }
}

Concurrency Utilities for .NET
==============================

This repository provides a set of useful concurrency utilities designed to simplify the management of retries, parallel processing, and caching in .NET applications. These utilities are optimized for multi-threaded environments, enabling better performance and more reliable applications.

Features
--------

1.  **Task Scheduler with Retry**\
    A utility for managing task execution with retry logic, exponential backoff, and timeouts. It helps ensure that tasks that fail due to transient issues are retried automatically, improving reliability.

2.  **Parallel Batch Processor**\
    A tool for parallelizing batch processing operations. It allows you to process large datasets concurrently, optimizing performance for operations that can be executed in parallel.

3.  **Advanced Concurrent Cache**\
    A high-performance, thread-safe caching solution with support for item expiration, priority-based eviction, and cache item dependencies. It ensures efficient memory management and fast access to frequently used data.

Dependency Injection Integration
--------------------------------

Each of the concurrency utilities has been updated to support Dependency Injection (DI) in .NET. You can now easily inject the services into your application components, promoting cleaner, more modular code.

### 1\. **Task Scheduler with Retry**

The **Task Scheduler with Retry** utility helps execute tasks with automatic retries, exponential backoff, and optional timeouts. It's useful for operations that might fail intermittently, such as network calls or database queries.

#### Key Features:

-   Retry logic with exponential backoff and jitter.
-   Timeout support to prevent tasks from hanging indefinitely.
-   Custom retry conditions and optional callbacks for logging retries.
-   Graceful cancellation using cancellation tokens.

#### Example Usage

```
public class MyService
{
    private readonly ITaskSchedulerWithRetry _taskScheduler;

    public MyService(ITaskSchedulerWithRetry taskScheduler)
    {
        _taskScheduler = taskScheduler;
    }

    public async Task CallExternalApiAsync()
    {
        var result = await _taskScheduler.ExecuteWithRetryAsync(() => MakeHttpRequest(), maxRetryAttempts: 5, delayBetweenRetries: TimeSpan.FromSeconds(2));
    }
}
```

### 2\. **Parallel Batch Processor**

This utility allows you to process large datasets in parallel, making it ideal for batch processing scenarios.

#### Key Features:

-   Parallel processing of batches of items, improving performance on large datasets.
-   Option to use async methods or sync actions.
-   Cancellation token support for graceful termination of tasks.
-   Optimized for high concurrency scenarios.

#### Example Usage

```
public class MyBatchProcessorService
{
    private readonly IParallelBatchProcessor _batchProcessor;

    public MyBatchProcessorService(IParallelBatchProcessor batchProcessor)
    {
        _batchProcessor = batchProcessor;
    }

    public async Task ProcessDataAsync()
    {
        var items = new List<int> { 1, 2, 3 };
        await _batchProcessor.ProcessBatchAsync(items, async item => await ProcessItemAsync(item));
    }
}
```

### 3\. **Advanced Concurrent Cache**

A concurrent, thread-safe cache with advanced features such as item expiration, priority-based eviction, and cache item dependencies. This cache is designed for high-performance multi-threaded scenarios.

#### Key Features:

-   Expiration: Cache items can expire after a specified duration.
-   Priority-Based Eviction: Items with lower priority are evicted first when the cache exceeds its size limit.
-   Cache Dependencies: Defines relationships between cache items, ensuring that removing a parent item also removes dependent items.
-   Automatic Cleanup: Periodic cleanup of expired items to free memory.
-   Cache Statistics: Provides cache hits and misses for monitoring cache usage.

#### Example Usage

```
public class CacheService
{
    private readonly IAdvancedConcurrentCache<string, string> _cache;

    public CacheService(IAdvancedConcurrentCache<string, string> cache)
    {
        _cache = cache;
    }

    public void UseCache()
    {
        _cache.AddOrUpdate("item1", "value1");
        if (_cache.TryGetValue("item1", out var value))
        {
            Console.WriteLine(value); // Outputs: value1
        }
    }
}
```


Setting Up Dependency Injection (DI)
------------------------------------

To enable Dependency Injection, you need to register the services in the DI container. In an ASP.NET Core application, you can register the services in the `Startup.cs` or `Program.cs` file.

### Register Services


    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTaskSchedulerWithRetry();
        services.AddParallelBatchProcessor();
        services.AddAdvancedConcurrentCache();
    }


After registering these services, you can inject them into controllers or other services in your application, as shown in the examples above.

License
-------

Concurrency Utilities is Copyright © 2025 Momen Shaker and other contributors under the MIT license.

Contributing
------------

Feel free to fork the repository, create a pull request, and submit issues for any bugs or enhancements. Contributions are welcome!

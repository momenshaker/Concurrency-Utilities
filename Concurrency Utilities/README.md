# Concurrency Utilities for .NET

This repository provides a set of useful concurrency utilities designed to simplify the management of retries, parallel processing, and caching in .NET applications. These utilities are optimized for multi-threaded environments, enabling better performance and more reliable applications.

## Features

1. **Task Scheduler with Retry**  
   A utility for managing task execution with retry logic, exponential backoff, and timeouts. It helps ensure that tasks that fail due to transient issues are retried automatically, improving reliability.

2. **Parallel Batch Processor**  
   A tool for parallelizing batch processing operations. It allows you to process large datasets concurrently, optimizing performance for operations that can be executed in parallel.

3. **Advanced Concurrent Cache**  
   A high-performance, thread-safe caching solution with support for item expiration, priority-based eviction, and cache item dependencies. It ensures efficient memory management and fast access to frequently used data.

## Summary

### 1. **Task Scheduler with Retry**

The **Task Scheduler with Retry** class helps execute tasks with automatic retries, exponential backoff, and optional timeouts. It's useful when you're working with operations that might fail intermittently, such as network calls or database queries. With this utility, you can configure retry attempts, delay times, backoff factor, and a timeout to avoid hanging operations.

**Key Features:**
- Retry logic with exponential backoff and jitter.
- Timeout support to prevent tasks from hanging indefinitely.
- Custom retry conditions and optional callbacks for logging retries.
- Graceful cancellation using cancellation tokens.

### Use Case:  
You’re developing a microservice that makes HTTP requests to an external API. Occasionally, the API might experience temporary outages. Instead of failing immediately, the service should retry the request a few times with a delay to handle transient failures gracefully.

```csharp
var taskScheduler = new TaskSchedulerWithRetry();
var result = await taskScheduler.ExecuteWithRetryAsync(() => MakeHttpRequest(), maxRetryAttempts: 5, delayBetweenRetries: TimeSpan.FromSeconds(2));


### 2. **Parallel Batch Processor**

This utility allows you to process large sets of data in parallel, making it ideal for batch processing scenarios. It supports both async and sync operations.

**Key Features:**
- Parallel processing of batches of items, improving performance on large datasets.
- Option to use async methods or sync actions.
- Cancellation token support for graceful termination of tasks.
- Optimized for high concurrency scenarios where tasks need to run concurrently.

```csharp
var processor = new ParallelBatchProcessor();
await processor.ProcessBatchAsync(new List<int> { 1, 2, 3 }, async item => await ProcessItemAsync(item));


### 3. **Advanced Concurrent Cache**

A concurrent, thread-safe cache with advanced features such as item expiration, priority-based eviction, and cache item dependencies. This cache is designed to be highly performant in multi-threaded scenarios.

**Key Features:**
- Expiration: Cache items can expire after a specified duration.
- Priority-Based Eviction: Items with lower priority are evicted first when the cache exceeds its size limit.
- Cache Dependencies: Allows defining relationships between cache items, ensuring that removing a parent item also removes dependent items.
- Automatic Cleanup: Periodic cleanup of expired items to free memory.
- Cache Statistics: Provides cache hits and misses for monitoring cache usage.

```csharp
var cache = new AdvancedConcurrentCache<string, string>(maxSize: 100);
cache.AddOrUpdate("item1", "value1");
cache.TryGetValue("item1", out var cachedValue);
Console.WriteLine(cachedValue); // Outputs: value1

## Usage

Each utility is provided as a standalone class, which can be used directly within your application. Please refer to the Example Usage sections for each utility.

### Task Scheduler with Retry
Use this class when you need to manage retry logic for tasks that might fail and require retries, with exponential backoff for better handling of transient failures.

### Parallel Batch Processor
This is useful when you need to process items in parallel, such as processing large batches of data, handling large-scale background tasks, or executing time-consuming operations concurrently.

### Advanced Concurrent Cache
This is perfect for situations where you need to cache data in a thread-safe manner, handle data expiration, control memory usage with eviction policies, and define dependencies between cached items.

## License

Concurrency Utilities is Copyright © 2025 Momen Shaker and other contributors under the MIT license.

## Contributing

Feel free to fork the repository, create a pull request, and submit issues for any bugs or enhancements. Contributions are welcome!


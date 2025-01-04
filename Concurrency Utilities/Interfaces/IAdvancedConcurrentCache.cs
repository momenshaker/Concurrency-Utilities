using Concurrency_Utilities.Enums;

namespace Concurrency_Utilities.Interfaces;

/// <summary>
/// Defines the interface for an advanced, thread-safe, concurrent cache with expiration, eviction, 
/// dependencies, and asynchronous operations.
/// </summary>
public interface IAdvancedConcurrentCache
{
    /// <summary>
    /// Event that is triggered when an item is added to the cache.
    /// </summary>
    event Action<object, object> ItemAdded;

    /// <summary>
    /// Event that is triggered when an item is updated in the cache.
    /// </summary>
    event Action<object, object> ItemUpdated;

    /// <summary>
    /// Event that is triggered when an item is removed from the cache.
    /// </summary>
    event Action<object> ItemRemoved;

    /// <summary>
    /// Gets the total number of items in the cache.
    /// </summary>
    int TotalItems { get; }

    /// <summary>
    /// Gets the total number of cache hits.
    /// </summary>
    int CacheHits { get; }

    /// <summary>
    /// Gets the total number of cache misses.
    /// </summary>
    int CacheMisses { get; }

    /// <summary>
    /// Asynchronously adds or updates an item in the cache with a specified expiration time and priority.
    /// </summary>
    Task AddOrUpdateAsync(object key, object value, TimeSpan? expiryTime = null, CachePriority priority = CachePriority.Normal);

    /// <summary>
    /// Tries to retrieve an item from the cache. Returns true if found, false if not found or expired.
    /// </summary>
    bool TryGetValue(object key, CancellationToken cancellationToken, out object value);

    /// <summary>
    /// Asynchronously removes an item from the cache and invalidates any dependent items.
    /// </summary>
    Task<bool> RemoveAsync(object key);

    /// <summary>
    /// Asynchronously clears all items from the cache.
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Asynchronously adds a dependency between two cache items.
    /// If the parent item is removed, the dependent items are also removed.
    /// </summary>
    Task AddDependencyAsync(object key, object dependentKey);
}

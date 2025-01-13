using Concurrency_Utilities.Enums;
using Concurrency_Utilities.Interfaces;
using System.Collections.Concurrent;

public class AdvancedConcurrentCache : IAdvancedConcurrentCache
{
    private readonly ConcurrentDictionary<object, CacheItem<object>> cache = new();
    private readonly int _maxSize;
    private readonly Timer _cleanupTimer;
    private readonly Dictionary<object, List<object>> dependencies = new();

    /// <summary>
    /// Event triggered when an item is added to the cache.
    /// </summary>
    public event Action<object, object> ItemAdded;

    /// <summary>
    /// Event triggered when an item in the cache is updated.
    /// </summary>
    public event Action<object, object> ItemUpdated;

    /// <summary>
    /// Event triggered when an item is removed from the cache.
    /// </summary>
    public event Action<object> ItemRemoved;

    /// <summary>
    /// Gets the total number of items currently in the cache.
    /// </summary>
    public int TotalItems => cache.Count;

    /// <summary>
    /// Gets the total number of cache hits.
    /// </summary>
    public int CacheHits { get; private set; }

    /// <summary>
    /// Gets the total number of cache misses.
    /// </summary>
    public int CacheMisses { get; private set; }

    /// <summary>
    /// Initializes a new instance of the AdvancedConcurrentCache with the specified max size and optional cleanup interval.
    /// </summary>
    /// <param name="maxSize">The maximum number of items the cache can hold before eviction occurs.</param>
    /// <param name="cleanupInterval">The interval at which expired items will be cleaned up. Defaults to 5 minutes.</param>
    public AdvancedConcurrentCache(int maxSize = 100, TimeSpan? cleanupInterval = null)
    {
        _maxSize = maxSize;
        _cleanupTimer = new Timer(async _ => await CleanupExpiredItemsAsync(), null, TimeSpan.Zero, cleanupInterval ?? TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Adds or updates an item in the cache with an optional expiration time and cache priority.
    /// </summary>
    /// <param name="key">The key of the item to add or update.</param>
    /// <param name="value">The value to be added or updated.</param>
    /// <param name="expiryTime">The expiration time for the cache item.</param>
    /// <param name="priority">The cache priority for eviction purposes.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddOrUpdateAsync(object key, object value, TimeSpan? expiryTime = null, CachePriority priority = CachePriority.Normal)
    {
        try
        {
            var cacheItem = new CacheItem<object>(value, expiryTime, priority);

            await Task.Run(() =>
            {
                cache.AddOrUpdate(key, cacheItem, (k, v) =>
                {
                    ItemUpdated?.Invoke(k, value);
                    return cacheItem;
                });

                if (!cache.ContainsKey(key))
                {
                    ItemAdded?.Invoke(key, value);
                }

                if (cache.Count > _maxSize)
                {
                    EvictItemsAsync().ConfigureAwait(false);
                }
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in AddOrUpdateAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Tries to retrieve a value from the cache for the specified key.
    /// </summary>
    /// <param name="key">The key to retrieve the cached value for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="value">The cached value, if found.</param>
    /// <returns>True if the value was found and is not expired; otherwise, false.</returns>
    public bool TryGetValue(object key, CancellationToken cancellationToken, out object value)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (cache.TryGetValue(key, out var cacheItem) && !cacheItem.IsExpired)
            {
                CacheHits++;
                cacheItem.Refresh();
                value = cacheItem.Value;
                return true;
            }
            else
            {
                CacheMisses++;
                value = null;
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in TryGetValueAsync: {ex.Message}");
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Removes an item from the cache asynchronously, including any dependent items.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    /// <returns>A task that represents the asynchronous operation, returning true if the item was removed successfully.</returns>
    public async Task<bool> RemoveAsync(object key)
    {
        try
        {
            return await Task.Run(async () =>
            {
                if (cache.TryRemove(key, out var cacheItem))
                {
                    ItemRemoved?.Invoke(key);

                    if (dependencies.ContainsKey(key))
                    {
                        foreach (var dependentKey in dependencies[key])
                        {
                            await RemoveAsync(dependentKey);
                        }
                        dependencies.Remove(key);
                    }
                    return true;
                }

                return false;
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in RemoveAsync: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clears all items and dependencies from the cache.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ClearAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                cache.Clear();
                dependencies.Clear();
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in ClearAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Adds a dependency between two cache items. If the parent item is removed, the dependent item will also be removed.
    /// </summary>
    /// <param name="key">The key of the parent item.</param>
    /// <param name="dependentKey">The key of the dependent item.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddDependencyAsync(object key, object dependentKey)
    {
        try
        {
            await Task.Run(() =>
            {
                if (!dependencies.ContainsKey(key))
                {
                    dependencies[key] = new List<object>();
                }

                dependencies[key].Add(dependentKey);
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in AddDependencyAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Cleans up expired items from the cache asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task CleanupExpiredItemsAsync()
    {
        try
        {
            await Task.Run(async () =>
            {
                foreach (var key in cache.Keys.ToList())
                {
                    if (cache.TryGetValue(key, out var cacheItem) && cacheItem.IsExpired)
                    {
                        await RemoveAsync(key);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in CleanupExpiredItemsAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Evicts items from the cache to ensure the cache size does not exceed the maximum size.
    /// Evicts items based on their priority and last accessed time.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task EvictItemsAsync()
    {
        try
        {
            var itemsToEvict = cache.OrderBy(kvp => kvp.Value.Priority)
                                      .ThenBy(kvp => kvp.Value.LastAccessed)
                                      .Take(cache.Count - _maxSize)
                                      .Select(kvp => kvp.Key)
                                      .ToList();

            foreach (var key in itemsToEvict)
            {
                await RemoveAsync(key);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in EvictItemsAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Represents an individual cache item with associated metadata (value, last accessed time, expiration time, priority).
    /// </summary>
    private class CacheItem<T>
    {
        public CacheItem(T value, TimeSpan? expiryTime, CachePriority priority)
        {
            Value = value;
            ExpiryTime = expiryTime;
            Priority = priority;
            LastAccessed = DateTime.UtcNow;
        }

        public T Value { get; }
        public DateTime LastAccessed { get; private set; }
        public TimeSpan? ExpiryTime { get; }
        public CachePriority Priority { get; }
        public bool IsExpired => ExpiryTime.HasValue && DateTime.UtcNow - LastAccessed > ExpiryTime.Value;

        /// <summary>
        /// Refreshes the last accessed time of the cache item.
        /// </summary>
        public void Refresh() => LastAccessed = DateTime.UtcNow;
    }
}

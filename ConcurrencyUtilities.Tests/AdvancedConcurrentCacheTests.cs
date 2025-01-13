using Xunit;

public class AdvancedConcurrentCacheTests
{
    [Fact]
    public async Task AddOrUpdateAsync_ShouldAddItem_WhenItemIsNotPresent()
    {
        // Arrange
        var cache = new AdvancedConcurrentCache(maxSize: 3);
        var key = "item1";
        var value = "value1";

        // Act
        await cache.AddOrUpdateAsync(key, value);

        // Assert
        var result = cache.TryGetValue(key, CancellationToken.None, out var cachedValue);
        Assert.True(result);
        Assert.Equal(value, cachedValue);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldUpdateItem_WhenItemAlreadyExists()
    {
        // Arrange
        var cache = new AdvancedConcurrentCache(maxSize: 3);
        var key = "item1";
        var value1 = "value1";
        var value2 = "value2";
        await cache.AddOrUpdateAsync(key, value1);

        // Act
        await cache.AddOrUpdateAsync(key, value2);

        // Assert
        var result = cache.TryGetValue(key, CancellationToken.None, out var cachedValue);
        Assert.True(result);
        Assert.Equal(value2, cachedValue);
    }

    [Fact]
    public async Task TryGetValue_ShouldReturnFalse_WhenItemIsExpired()
    {
        // Arrange
        var cache = new AdvancedConcurrentCache(maxSize: 3);
        var key = "item1";
        var value = "value1";
        var expiryTime = TimeSpan.FromMilliseconds(1); // Expire quickly
        await cache.AddOrUpdateAsync(key, value, expiryTime);

        // Act
        await Task.Delay(2); // Wait for the item to expire
        var result = cache.TryGetValue(key, CancellationToken.None, out var cachedValue);

        // Assert
        Assert.False(result);
        Assert.Null(cachedValue);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveItem_WhenItemExists()
    {
        // Arrange
        var cache = new AdvancedConcurrentCache(maxSize: 3);
        var key = "item1";
        var value = "value1";
        await cache.AddOrUpdateAsync(key, value);

        // Act
        var removed = await cache.RemoveAsync(key);

        // Assert
        Assert.True(removed);
        var result = cache.TryGetValue(key, CancellationToken.None, out var cachedValue);
        Assert.False(result);
        Assert.Null(cachedValue);
    }

    [Fact]
    public async Task EvictItemsAsync_ShouldEvictItems_WhenCacheExceedsMaxSize()
    {
        // Arrange
        var cache = new AdvancedConcurrentCache(maxSize: 2);
        var key1 = "item1";
        var value1 = "value1";
        var key2 = "item2";
        var value2 = "value2";
        var key3 = "item3";
        var value3 = "value3";

        await cache.AddOrUpdateAsync(key1, value1);
        await cache.AddOrUpdateAsync(key2, value2);
        await cache.AddOrUpdateAsync(key3, value3); // At this point, eviction will occur

        // Act
        var result1 = cache.TryGetValue(key1, CancellationToken.None, out var cachedValue1);
        var result2 = cache.TryGetValue(key2, CancellationToken.None, out var cachedValue2);
        var result3 = cache.TryGetValue(key3, CancellationToken.None, out var cachedValue3);

        // Assert
        // One of the keys (item1, item2, or item3) should be evicted, so one result should be false
        Assert.False(result1 && result2 && result3); // One of the items should be evicted due to max size
    }

    [Fact]
    public async Task AddDependencyAsync_ShouldCascadeRemoveDependentItems_WhenParentItemIsRemoved()
    {
        // Arrange
        var cache = new AdvancedConcurrentCache(maxSize: 3);
        var parentKey = "parentItem";
        var dependentKey = "dependentItem";
        var parentValue = "parentValue";
        var dependentValue = "dependentValue";

        // Add items to cache
        await cache.AddOrUpdateAsync(parentKey, parentValue);
        await cache.AddOrUpdateAsync(dependentKey, dependentValue);

        // Add dependency
        await cache.AddDependencyAsync(parentKey, dependentKey);

        // Act
        var removedParent = await cache.RemoveAsync(parentKey);

        // Assert
        Assert.True(removedParent);
        var result = cache.TryGetValue(dependentKey, CancellationToken.None, out var cachedDependentValue);
        Assert.False(result); // Dependent item should be removed when parent item is removed
    }

    [Fact]
    public async Task ClearAsync_ShouldClearAllItemsAndDependencies()
    {
        // Arrange
        var cache = new AdvancedConcurrentCache(maxSize: 3);
        var key1 = "item1";
        var key2 = "item2";
        var value1 = "value1";
        var value2 = "value2";

        // Add items to cache
        await cache.AddOrUpdateAsync(key1, value1);
        await cache.AddOrUpdateAsync(key2, value2);

        // Act
        await cache.ClearAsync();

        // Assert
        var result1 = cache.TryGetValue(key1, CancellationToken.None, out var cachedValue1);
        var result2 = cache.TryGetValue(key2, CancellationToken.None, out var cachedValue2);
            
        Assert.False(result1); // Item 1 should be removed
        Assert.False(result2); // Item 2 should be removed
    }
}

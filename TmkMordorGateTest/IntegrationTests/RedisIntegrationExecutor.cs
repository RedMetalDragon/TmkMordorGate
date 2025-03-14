using Microsoft.Extensions.Caching.Distributed;
using TmkMordorGateTest.Setup;
using TmkMordorGate.Helpers;
using TmkMordorGate.Models;

namespace TmkMordorGateTest.IntegrationTests;

public class RedisIntegrationExecutor : IClassFixture<TmkTestFixture>, IAsyncDisposable
{
    private readonly TmkTestFixture _fixture;
    private IDistributedCache _cache;

    public RedisIntegrationExecutor(TmkTestFixture fixture)
    {
        _fixture = fixture;
        _cache = fixture.GetCache();
    }

    
    public async ValueTask DisposeAsync()
    {
        await _fixture.ClearCache();
    }
    
    [Fact]
    public async Task GetOrSetAsync_WithNullValue_ShouldReturnNull()
    {
        // Arrange
        const string key = "null_test";

        // Act
        var result = await _cache.GetOrSetAsync(key, async () => (RedisRecord)null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrSetAsync_WithCustomOptions_ShouldRespectOptions()
    {
        // Arrange
        const string key = "custom_options_test";
        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(1));
        var testModel = new RedisRecord { Id = 1, Name = "Test" };

        // Act
        await _cache.GetOrSetAsync(key, async () => testModel, options);
        await Task.Delay(2000); // Wait for expiration
        var result =
            await _cache.GetOrSetAsync(key, async () => new RedisRecord { Id = 2, Name = "Different" });

        // Assert
        Assert.NotEqual(testModel.Id, result.Id);
    }

    [Fact]
    public async Task GetOrSetAsync_WithComplexObject_ShouldPreserveProperties()
    {
        // Arrange
        const string key = "complex_object_test";
        var testModel = new RedisRecord
        {
            Id = 1,
            Name = "Test",
            Items = new List<string> { "item1", "item2" },
            CreatedDate = DateTime.UtcNow
        };

        // Act
        await _cache.GetOrSetAsync(key, async () => testModel);
        var result = await _cache.GetOrSetAsync(key, async () => new RedisRecord());

        // Assert
        Assert.Equal(testModel.Id, result.Id);
        Assert.Equal(testModel.Name, result.Name);
        Assert.Equal(testModel.Items, result.Items);
        Assert.Equal(testModel.CreatedDate, result.CreatedDate);
    }

    [Fact]
    public async Task GetOrSetAsync_WithConcurrentCalls_ShouldReturnSameValue()
    {
        // Arrange
        const string key = "concurrent_test";
        var testModel = new RedisRecord { Id = 1, Name = "Test" };

        // Act
        var tasks = new List<Task<RedisRecord>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_cache.GetOrSetAsync(key, async () =>
            {
                await Task.Delay(100).ConfigureAwait(false); // Simulate work
                return testModel;
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, result => Assert.Equal(testModel.Id, result.Id));
    }

    [Fact]
    public async Task GetOrSetAsync_WithNonExistentKey_ShouldCallFactory()
    {
        // Arrange
        const string key = "factory_test";
        var factoryCalled = false;

        // Act
        var result = await _cache.GetOrSetAsync(key, async () =>
        {
            factoryCalled = true;
            return new RedisRecord() { Id = 1 };
        });

        // Assert
        Assert.True(factoryCalled);
        Assert.NotNull(result);
    }
    
}
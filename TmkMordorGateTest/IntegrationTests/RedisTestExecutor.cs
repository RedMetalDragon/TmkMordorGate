using Microsoft.Extensions.Caching.Distributed;
using TmkMordorGateTest.Setup;
using TmkMordorGate.Helpers;
namespace TmkMordorGateTest.IntegrationTests;

public class RedisTestExecutor: IAsyncDisposable
{
    
    private class TestModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<string>? Items { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    
    public RedisTestExecutor()
    {
        _setup = new TmkRedisIntegrationTestSetup();
        _cache = _setup.Cache;
    }
    
    private readonly TmkRedisIntegrationTestSetup _setup;
    private readonly IDistributedCache _cache;
    
    [Fact]
    public async Task GetOrSetAsync_WithNullValue_ShouldReturnNull()
    {
        // Arrange
        const string key = "null_test";

        // Act
        var result = await _cache.GetOrSetAsync(key, async () => (TestModel)null!);

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
        var testModel = new TestModel { Id = 1, Name = "Test" };

        // Act
        await _cache.GetOrSetAsync(key, async () => testModel, options);
        await Task.Delay(2000); // Wait for expiration
        var result = await _cache.GetOrSetAsync(key, async () => new TestModel { Id = 2, Name = "Different" });

        // Assert
        Assert.NotEqual(testModel.Id, result.Id);
    }

    [Fact]
    public async Task GetOrSetAsync_WithComplexObject_ShouldPreserveProperties()
    {
        // Arrange
        const string key = "complex_object_test";
        var testModel = new TestModel
        {
            Id = 1,
            Name = "Test",
            Items = new List<string> { "item1", "item2" },
            CreatedDate = DateTime.UtcNow
        };

        // Act
        await _cache.GetOrSetAsync(key, async () => testModel);
        var result = await _cache.GetOrSetAsync(key, async () => new TestModel());

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
        var testModel = new TestModel { Id = 1, Name = "Test" };

        // Act
        var tasks = new List<Task<TestModel>>();
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
            return new TestModel { Id = 1 };
        });

        // Assert
        Assert.True(factoryCalled);
        Assert.NotNull(result);
    }

    public async ValueTask DisposeAsync()
    {
        await _setup.ClearCache();
    }
}
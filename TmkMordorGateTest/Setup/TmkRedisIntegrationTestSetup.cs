using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TmkMordorGate.Config;

namespace TmkMordorGateTest.Setup;

public class TmkRedisIntegrationTestSetup
{
    public IConfiguration _configuration;
    public IDistributedCache Cache { get; private set; }

    public TmkRedisIntegrationTestSetup()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("test_settings_local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        Cache = GetDistributedCache();
    }
    
    private IDistributedCache GetDistributedCache()
    {
        var redisSettings = new TmkRedisCacheSettings
        {
            Host = _configuration["RedisCacheSettings:Host"] ?? "localhost",
            Port = _configuration["RedisCacheSettings:Port"] ?? "6379",
            Password = _configuration["RedisCacheSettings:Password"] ?? ""
        };

        var services = new ServiceCollection();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{redisSettings.Host}:{redisSettings.Port},password={redisSettings.Password}";
            options.InstanceName = "UnitTest_";
        });

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IDistributedCache>();
    }

    public async Task ClearCache()
    {
        var connection = ConnectionMultiplexer.Connect($"{_configuration["RedisCacheSettings:Host"]}:{_configuration["RedisCacheSettings:Port"]},password={_configuration["RedisCacheSettings:Password"]}");
        var server = connection.GetServer(connection.GetEndPoints().First());
        await server.FlushDatabaseAsync();
    }
}
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TmkMordorGate.Config;
using TmkMordorGate.DbContext;
using TmkMordorGate.Repositories;
using TmkMordorGate.Services;
using TmkMordorGateTest.Mocks;

namespace TmkMordorGateTest.Setup
{
    public class TmkTestFixture
    {
        public IServiceProvider ServiceProvider { get; }
        public IConfiguration Configuration { get; }

        public TmkTestFixture()
        {
            // Build configuration from test_settings_local.json (or environment variables)
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("test_settings_local.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = configurationBuilder.Build();

            // Build the service collection with required dependencies
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            // Register configuration
            services.AddSingleton<IConfiguration>(Configuration);

            // Register database settings
            services.AddSingleton<IDatabaseSettings, TmkMySqlDatabaseSettings>();

            // Register DbContext using test settings
            services.AddDbContext<TimeKeeperDbContext>(options =>
            {
                var dbSettings = new TmkMySqlDatabaseSettings();
                Configuration.GetSection("DatabaseSettings").Bind(dbSettings);
                // TmkMySqlDatabaseSettings.FromJdbcUrl builds a connection string from the JDBC URL
                options.UseMySql(
                    TmkMySqlDatabaseSettings.FromJdbcUrl(dbSettings.Host, dbSettings.Username, dbSettings.Password).ConnectionString,
                    new MySqlServerVersion(new Version(8, 0, 21))
                );
            });

            // Register repository – you can replace FakeAccessControlRepository with your actual repository if needed.
            services.AddScoped<IAuthenticationAuthorizationRepository, FakeAccessControlRepository>();
            services.AddScoped<TmkAccessControlRepository, TmkAccessControlRepository>();

            // Register the authentication service using the repository and a mocked configuration service if needed
            services.AddScoped<TmkAuthenticationService, TmkAuthenticationService>();

            // Register Redis distributed cache
            services.AddStackExchangeRedisCache(options =>
            {
                var redisSettings = new TmkRedisCacheSettings
                {
                    Host = Configuration["RedisCacheSettings:Host"] ?? "localhost",
                    Port = Configuration["RedisCacheSettings:Port"] ?? "6379",
                    Password = Configuration["RedisCacheSettings:Password"] ?? ""
                };
                options.Configuration = $"{redisSettings.Host}:{redisSettings.Port},password={redisSettings.Password}";
                options.InstanceName = "UnitTest_";
            });

            // Optionally, register other services such as authorization factories
            services.AddScoped<IAuthorizationFactory, FakeAuthorizationFactory>();

            // Build the service provider
            ServiceProvider = services.BuildServiceProvider();
        }

        // Helper method to retrieve the DbContext
        public TimeKeeperDbContext CreateDbContext() =>
            ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<TimeKeeperDbContext>();

        // Helper method to retrieve the distributed cache
        public IDistributedCache GetCache() =>
            ServiceProvider.GetRequiredService<IDistributedCache>();

        // Helper method to retrieve the authentication service
        public TmkAuthenticationService GetAuthenticationService() =>
            ServiceProvider.GetRequiredService<TmkAuthenticationService>();

        // Helper method to retrieve the access control repository
        public TmkAccessControlRepository GetAccessControlRepository() =>
            ServiceProvider.GetRequiredService<TmkAccessControlRepository>();
        
        public IServiceScope CreateScope() => ServiceProvider.CreateScope();
        
        // Helper method to clear the Redis cache
        public async Task ClearCache()
        {
            var connection = ConnectionMultiplexer.Connect($"{Configuration["RedisCacheSettings:Host"]}:{Configuration["RedisCacheSettings:Port"]},password={Configuration["RedisCacheSettings:Password"]}");
            var server = connection.GetServer(connection.GetEndPoints().First());
            await server.FlushDatabaseAsync();
        }
        
    }
}
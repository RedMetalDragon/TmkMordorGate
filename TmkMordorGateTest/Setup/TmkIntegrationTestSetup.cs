using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TmkMordorGate.Config;
using TmkMordorGate.DbContext;
using TmkMordorGate.Repositories;
using TmkMordorGate.Services;
using TmkMordorGateTest.Mocks;

namespace TmkMordorGateTest.Setup;

public class TmkIntegrationTestSetup
{
    public IConfiguration _configuration;
    
    public TimeKeeperDbContext DbContext { get; private set; }
    public TmkAuthenticationService AuthenticationService { get; private set; }
    public TmkAccessControlRepository AccessControlRepository { get; private set; }
    
    public TmkIntegrationTestSetup()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("test_settings_local.json", optional: true) // Load local file if it exists
            .AddEnvironmentVariables() // Override with environment variables if set
            .Build();
        DbContext = GetTimeKeeperDbContext();
        AccessControlRepository = GetAuthorizationAuthenticationRepository();
        AuthenticationService = GetAuthenticationService();
    }

    private TimeKeeperDbContext GetTimeKeeperDbContext()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("test_settings_local.json", optional: true) // Load local file if it exists
            .AddEnvironmentVariables() // Override with environment variables if set
            .Build();
        var dbSettings = new TmkMySqlDatabaseSettings();
        config.GetSection("DatabaseSettings").Bind(dbSettings);
        var options = new DbContextOptionsBuilder<TimeKeeperDbContext>()
            .UseMySql(
                TmkMySqlDatabaseSettings.FromJdbcUrl(dbSettings.Host, dbSettings.Username, dbSettings.Password)
                    .ConnectionString, new MySqlServerVersion(new Version(8, 0, 21)))
            .Options;
        return new TimeKeeperDbContext(options, dbSettings);
    }

    private TmkAccessControlRepository GetAuthorizationAuthenticationRepository()
    {
        var context = GetTimeKeeperDbContext();
        return new TmkAccessControlRepository(context);
    }

    private TmkAuthenticationService GetAuthenticationService()
    {
        var repository = GetAuthorizationAuthenticationRepository();
        return new TmkAuthenticationService(repository, new MockMordorConfigurationService(_configuration));
    }
}
    
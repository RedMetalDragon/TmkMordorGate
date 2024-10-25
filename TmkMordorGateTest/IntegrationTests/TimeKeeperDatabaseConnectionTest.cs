using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TmkMordorGate.Config;

namespace TmkMordorGateTest.IntegrationTests;

using TmkMordorGate.DbContext;

public class TimeKeeperDatabaseConnectionTest
{
    private TimeKeeperDbContext GetTimeKeeperDbContext()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("test_settings_local.json", optional: true) // Load local file if it exists
            .AddEnvironmentVariables() // Override with environment variables if set
            .Build();
        var dbSettings = new DatabaseSettings();
        config.GetSection("DatabaseSettings").Bind(dbSettings);
        var options = new DbContextOptionsBuilder<TimeKeeperDbContext>()
            .UseMySql(
                DatabaseSettings.FromJdbcUrl(dbSettings.Host, dbSettings.Username, dbSettings.Password)
                    .ConnectionString, new MySqlServerVersion(new Version(8, 0, 21)))
            .Options;
        return new TimeKeeperDbContext(options, dbSettings);
    }


    [Fact]
    public void TestConnection()
    {
        // Arrange
        var context = GetTimeKeeperDbContext();

        // Act
        var canConnect = context.Database.CanConnect();

        // Assert
        Assert.True(canConnect, "Unable to connect to the database.");
    }
}
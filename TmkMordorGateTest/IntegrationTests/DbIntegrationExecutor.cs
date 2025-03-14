using Microsoft.Extensions.DependencyInjection;
using TmkMordorGate.DbContext;
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGateTest.Setup;

namespace TmkMordorGateTest.IntegrationTests;

public class DbIntegrationExecutor : IClassFixture<TmkTestFixture>
{
    private TmkTestFixture _testFixture;

    public DbIntegrationExecutor(TmkTestFixture fixture)
    {
        _testFixture = fixture;
    }
    
    [Fact]
    public void TestDatabaseConnection()
    {
        using var scope = _testFixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TimeKeeperDbContext>();
        var canConnect = context.Database.CanConnect();
        Assert.True(canConnect, "Unable to connect to the database.");
    }

    [Fact]
    public async Task TestGetNullForNotFoundUserById()
    {
        var testContext = new TmkTestFixture();
        var repository = testContext.GetAccessControlRepository();
        var invalidEmail = testContext.Configuration.GetSection("TestData:InvalidUserEmail").Value;
        if (invalidEmail != null)
        {
            var result = await repository.GetUser("test@email.com");
            // Assert
            Assert.Null(result);
        }
        else
        {
            Assert.Fail("Invalid user email not found in test settings (JSON file)");
        }
    }
    
    [Fact]
    public async Task TestGetUserByEmail()
    {
        using var scope = _testFixture.ServiceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAuthenticationAuthorizationRepository>();
        var validEmail = _testFixture.Configuration.GetSection("TestData:ValidUserEmail").Value;
        if (validEmail != null)
        {
            var result = await repository.GetUser(validEmail);
            Assert.NotNull(result);
        }
        else
        {
            Assert.Fail("Valid user email not found in test settings (JSON file)");
        }
    }
}
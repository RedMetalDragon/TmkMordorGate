using TmkMordorGateTest.Setup;

namespace TmkMordorGateTest.IntegrationTests;

using Xunit;

public class DbTestExecutor
{
    private readonly TmkIntegrationTestSetup _integrationTestSetup;

    public DbTestExecutor()
    {
        _integrationTestSetup = new TmkIntegrationTestSetup();
    }

    [Fact]
    public void TestDatabaseConnection()
    {
        var context = _integrationTestSetup.DbContext;
        var canConnect = context.Database.CanConnect();
        // Assert
        Assert.True(canConnect, "Unable to connect to the database.");
    }

    [Fact]
    public async Task TestGetNullForNotFoundUserById()
    {
        var repository = _integrationTestSetup.AuthenticationRepository;
        var invalidEmail = _integrationTestSetup._configuration.GetSection("TestData:InvalidUserEmail").Value;
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
        var repository = _integrationTestSetup.AuthenticationRepository;
        var validEmail = _integrationTestSetup._configuration.GetSection("TestData:ValidUserEmail").Value;
        if (validEmail != null)
        {
            var result = await repository.GetUser(validEmail);
            // Assert
            Assert.NotNull(result);
        }
        else
        {
            Assert.Fail("Valid user email not found in test settings (JSON file)");
        }
    }
}
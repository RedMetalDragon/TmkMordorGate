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
        var repository = _integrationTestSetup.AccessControlRepository;
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
        var repository = _integrationTestSetup.AccessControlRepository;
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
    
    // [Fact]
    // public async Task TestGetPermissions()
    // {
    //     var repository = _integrationTestSetup.AccessControlRepository;
    //     var roleId = int.Parse(_integrationTestSetup._configuration.GetSection("TestData:RoleId").Value);
    //     if (roleId != 0)
    //     {
    //         var result = await repository.GetPermissions(roleId);
    //         // Assert
    //         Assert.NotEmpty(result);
    //     }
    //     else
    //     {
    //         Assert.Fail("Role ID not found in test settings (JSON file)");
    //     }
    // }
    
    
    
}
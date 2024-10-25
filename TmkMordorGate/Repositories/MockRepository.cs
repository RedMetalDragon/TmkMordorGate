using TmkMordorGate.Models;
using TmkMordorGate.Repositories.Interfaces;

namespace TmkMordorGate.Repositories;

public class MockRepository : IAuthenticationRepository
{
    /// <summary>
    /// Mock user for testing purposes.
    /// </summary>
    /// <param name="emailAddress"> Email address </param>
    /// <returns></returns>
    public Task<Auth> GetUser(string emailAddress)
    {
        var user = new Auth
        {
            Email = emailAddress,
            PasswordHash = "password",
            Salt = "salt",
            Status = "Active",
            EmployeeID = 1,
            KeepLoggedIn = true,
            PasswordResetToken = "sda",
            PasswordResetTokenExpiration = DateTime.Now,
            AuthID = 1
        };
        return Task.FromResult(user);
    }
}
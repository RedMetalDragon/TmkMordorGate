using TmkMordorGate.Models;
using TmkMordorGate.Repositories.Interfaces;

namespace TmkMordorGate.Repositories;

public class AuthRepository : IAuthenticationRepository
{
    public Task<Auth> GetUser(string emailAddress)
    {
        throw new NotImplementedException();
    }
}
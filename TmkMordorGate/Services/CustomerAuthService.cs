using TmkMordorGate.Interfaces;
using TmkMordorGate.Models;

namespace TmkMordorGate.Services;

public class CustomerAuthService : ICustomerAuthService
{
    public Task<AuthenticadedResponse> Authenticate(AuthenticatedRequest model)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<UserAuth>> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<UserAuth> GetById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<UserAuth> Create(AuthenticatedRequest model)
    {
        throw new NotImplementedException();
    }

    public Task Update(int id, AuthenticatedRequest model)
    {
        throw new NotImplementedException();
    }

    public Task Delete(int id)
    {
        throw new NotImplementedException();
    }
}
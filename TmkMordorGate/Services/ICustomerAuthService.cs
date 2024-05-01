namespace TmkMordorGate.Interfaces;
using TmkMordorGate.Models;

public interface ICustomerAuthService
{
    Task<AuthenticadedResponse> Authenticate(AuthenticatedRequest model);
    Task<IEnumerable<UserAuth>> GetAll();
    Task<UserAuth> GetById(int id);
    Task<UserAuth> Create(AuthenticatedRequest model);
    Task Update(int id, AuthenticatedRequest model);
    Task Delete(int id);

}

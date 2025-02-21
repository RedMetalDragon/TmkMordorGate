using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Services.Authorization;

public class TmkAuthorizationServices: IAuthorizationService
{
    //private readonly IAuthenticationAuthorizationRepository _accessControlRepository;

    // public TmkAuthorizationServices(IAuthenticationAuthorizationRepository accessControlRepository)
    // {
    //     _accessControlRepository = accessControlRepository;
    // }

    public string GetServiceTarget()
    {
        return "TmkAuthorizationServices";
    }

    Task<bool> IAuthorizationService.Authorize(HttpContext context)
    {
        return Task.FromResult(false);
    }
}
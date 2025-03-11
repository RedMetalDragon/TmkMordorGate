using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Services.Authorization;

public class TmkAuthorizationServices : IAuthorizationService
{
    public string GetServiceTarget()
    {
        return "TmkAuthorizationServices";
    }

    Task<bool> IAuthorizationService.Authorize(HttpContext context)
    {
        return Task.FromResult(context.Request.Headers.ContainsKey("Authorization"));
    }
}
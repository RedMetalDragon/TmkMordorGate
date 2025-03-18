using System.Security.Claims;
using TmkMordorGate.Services.Interfaces;


namespace TmkMordorGate.Services.Authorization;

public class TmkFace : IAuthorizationService
{
    public Task<bool> Authorize(HttpContext context)
    {
        return Task.FromResult(true);
    }

    public string GetServiceTarget()
    {
        return "TmkFace";
    }
}
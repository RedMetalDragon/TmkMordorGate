using Microsoft.AspNetCore.Mvc;
using TmkMordorGate.Models;
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Services;

public class TmkAuthorizationServices: IAuthorizationService
{
    private readonly IAuthenticationAuthorizationRepository _accessControlRepository;

    public TmkAuthorizationServices(IAuthenticationAuthorizationRepository accessControlRepository)
    {
        _accessControlRepository = accessControlRepository;
    }
    
     
    public Task<IActionResult> Authorize(Employee? employee)
    {
        // Check if the employee is null
        if (employee == null)
        {
            return Task.FromResult<IActionResult>(new UnauthorizedResult());
        }
        // Check if the employee is authorized
        
        
        throw new NotImplementedException();
    }

    public Task<bool> IsAuthorized(string email, string password)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsAuthorized(string jwtToken)
    {
        throw new NotImplementedException();
    }

    public Task<IActionResult> Authorize(HttpContext context)
    {
        throw new NotImplementedException();
    }

    Task<bool> IAuthorizationService.Authorize(HttpContext context)
    {
        throw new NotImplementedException();
    }
}
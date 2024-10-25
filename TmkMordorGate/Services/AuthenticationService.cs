using Microsoft.AspNetCore.Mvc;
using TmkMordorGate.Models;
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAuthenticationRepository _authenticationRepository;

    public AuthenticationService(IAuthenticationRepository authenticationRepository)
    {
        _authenticationRepository = authenticationRepository;
    }

    /// <summary>
    /// Authenticates a user.
    /// </summary>
    public Task<IActionResult> Authenticate(AuthenticatedRequest model)
    {
        throw new NotImplementedException();
    }
}
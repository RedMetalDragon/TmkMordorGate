using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using TmkMordorGate.Models;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Controllers;

[ApiController]
[Route("api/v1/mordor")]
public class LoginController: ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public LoginController(IAuthenticationService authService)
    {
        _authenticationService = authService;
        
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            return await _authenticationService.Authenticate(request.Email, request.Password);
        }
        catch
        {
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
    
    
}
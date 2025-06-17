using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Models;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Controllers;

[ApiController]
[Route("api/v1/mordor")]
public class LoginController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IBlackListTokenService _blackListTokenService;

    public LoginController(IAuthenticationService authService, IBlackListTokenService blackListTokenService)
    {
        _authenticationService = authService;
        _blackListTokenService = blackListTokenService;
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

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return BadRequest("No token provided.");

        var tokenStr = authHeader.ToString().Replace("Bearer ", string.Empty);
        _blackListTokenService.RevokeToken(tokenStr);
        return Ok();
    }
}
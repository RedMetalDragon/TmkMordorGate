using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TmkMordorGate.Models;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Services.Authorization;

public class Dummy : IAuthorizationService
{
    public Task<IActionResult> Authorize(Employee? employee)
    {
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


    Task<bool> IAuthorizationService.Authorize(HttpContext context)
    {
        return Task.FromResult(context.Request.Headers.ContainsKey("Authorization"));
    }

    public Task<IActionResult> Authorize(HttpContext context)
    {
        var authorizedRequest = new AuthorizedRequest
        {
            IsAuthorized = true,
            Message = "Request is authorized"
        };
        return Task.FromResult<IActionResult>(new OkObjectResult(authorizedRequest));
    }
}
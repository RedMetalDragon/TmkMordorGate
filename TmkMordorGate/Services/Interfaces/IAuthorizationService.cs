using Microsoft.AspNetCore.Mvc;
using TmkMordorGate.Models;

namespace TmkMordorGate.Services.Interfaces;

public interface IAuthorizationService
{
    Task<bool> Authorize(HttpContext context);
    // Task<IActionResult> Authorize(Employee? employee);
    //
    // Task<bool> IsAuthorized(string email, string password);
    //
    // Task<bool> IsAuthorized(string jwtToken);
}
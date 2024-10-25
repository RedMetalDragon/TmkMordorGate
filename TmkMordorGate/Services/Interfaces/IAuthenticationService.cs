using Microsoft.AspNetCore.Mvc;
using TmkMordorGate.Models;

namespace TmkMordorGate.Services.Interfaces;

public interface IAuthenticationService
{
    Task<IActionResult> Authenticate(string email, string password);
}
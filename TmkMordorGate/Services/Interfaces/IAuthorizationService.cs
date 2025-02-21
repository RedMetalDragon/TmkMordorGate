using Microsoft.AspNetCore.Mvc;
using TmkMordorGate.Models;

namespace TmkMordorGate.Services.Interfaces;

public interface IAuthorizationService
{
    Task<bool> Authorize(HttpContext context);
    
    string GetServiceTarget();
}
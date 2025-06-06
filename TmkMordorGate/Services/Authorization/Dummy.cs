using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TmkMordorGate.Models;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Services.Authorization;

public class Dummy : IAuthorizationService
{
    Task<bool> IAuthorizationService.Authorize(HttpContext context)
    {
        return Task.FromResult(true);
    }

    public string GetServiceTarget()
    {
        return "Dummy";
    }
}
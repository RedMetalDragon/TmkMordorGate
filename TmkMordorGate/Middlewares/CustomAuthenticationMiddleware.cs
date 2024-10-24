using Microsoft.AspNetCore.Authentication;
using TmkMordorGate.Middlewares.Interfaces;
using TmkMordorGate.Services;

namespace TmkMordorGate.Middlewares;

public class CustomAuthenticationMiddleware : ISkipAuthentication
{
    private RequestDelegate _next;
    private readonly IEnumerable<string> _pathsToSkip;

    public CustomAuthenticationMiddleware(IMordorConfigurationService mordorConfigurationService, RequestDelegate next)
    {
        _next = next;
        _pathsToSkip = mordorConfigurationService.GetArrayOfConfigurationValue("_jwt_skip_path_");
    }

    public RequestDelegate Next
    {
        get => _next;
        set => _next = value;
    }

    public async Task Invoke(HttpContext context)
    {
        await SkipInvoke(context, _pathsToSkip);
    }
    public async Task SkipInvoke(HttpContext context, IEnumerable<string> pathToSkip)
    {
        if (_pathsToSkip.Any(path => context.Request.Path.ToString().Contains(path)))
        {
            await _next(context);
        }
        else
        {
            await context.ChallengeAsync();
            await _next(context);
        }
    }
}
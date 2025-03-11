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

    /// <summary>
    ///   The SkipInvoke function is used to skip authentication for the paths specified in the
    ///  _jwt_skip_path_ configuration value. If the path is not in the list of paths to skip or the path have the /api/v prefix, the
    ///  function will call the ChallengeAsync method to authenticate the request.
    /// </summary>
    /// <param name="context">HttpContext</param>
    /// <param name="pathToSkip">IEnumerable of strings containing paths where not authentication is required</param>
    public async Task SkipInvoke(HttpContext context, IEnumerable<string> pathToSkip)
    {
        if (_pathsToSkip.Any(path => context.Request.Path.ToString().Contains(path)) ||
            !context.Request.Path.ToString().Contains("/api/v"))
        {
            await _next(context);
        }
        else if (context.Request.Method == "post" && context.Request.Path.ToString() == "api/v1/mordor/users/login")
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
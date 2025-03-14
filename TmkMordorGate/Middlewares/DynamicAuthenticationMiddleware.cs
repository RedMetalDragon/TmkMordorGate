using Microsoft.AspNetCore.Authentication;
using TmkMordorGate.Middlewares.Interfaces;
using TmkMordorGate.Services;

namespace TmkMordorGate.Middlewares;

public class DynamicAuthenticationMiddleware : ISkipAuthentication
{
    private RequestDelegate _next;
    private readonly IEnumerable<string> _pathsToSkip;

    public DynamicAuthenticationMiddleware(IMordorConfigurationService mordorConfigurationService, RequestDelegate next)
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
        // Skip authentication for the login route.
        if (context.Request.Method == "POST" && context.Request.Path.Value.Contains("/api/v1/mordor/login"))
        {
            await _next(context);
            return;
        }

        // Authenticate the request (checks the Authorization header token)
        var authResult = await context.AuthenticateAsync();

        if (authResult is { Succeeded: true, Principal: not null })
        {
            // User is authenticated; continue processing.
            await _next(context);
        }
        else
        {
            // User is not authenticated; issue a challenge and do not continue.
            await context.ChallengeAsync();
            return;
        }
    }
}
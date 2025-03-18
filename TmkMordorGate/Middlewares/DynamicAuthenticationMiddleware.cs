using TmkMordorGate.Services;
using TmkMordorGate.Services.Interfaces;
using IMiddleware = TmkMordorGate.Middlewares.Interfaces.IMiddleware;

namespace TmkMordorGate.Middlewares;

public class DynamicAuthenticationMiddleware : IMiddleware
{
    private RequestDelegate _next;
    private readonly IEnumerable<string> _pathsToSkip;
    private IAuthenticationService _authenticationService;

    public DynamicAuthenticationMiddleware(IMordorConfigurationService mordorConfigurationService,
        IAuthenticationService authenticationService, RequestDelegate next)
    {
        _next = next;
        _pathsToSkip = mordorConfigurationService.GetArrayOfConfigurationValue("_jwt_skip_path_");
        _authenticationService = authenticationService;
    }

    public RequestDelegate? Next { get; set; }

    public async Task Invoke(HttpContext context)
    {
        var requestPath = context.Request.Path.Value ?? string.Empty;
        // Always bypass authentication for static files/non-API routes
        if (!requestPath.Contains("/api/v1/"))
        {
            await _next(context);
            return;
        }

        // Skip authentication for the login route and logout route
        if (context.Request.Method == "POST" && (context.Request.Path.Value.Contains("/api/v1/mordor/login") ||
                                                 context.Request.Path.Value.Contains("/api/v1/mordor/logout")))
        {
            await _next(context);
            return;
        }

        // Authenticate the request (checks the Authorization header token)
        var authResult = await _authenticationService.IsAuthenticated(context);

        if (authResult)
        {
            // User is authenticated; continue.
            await _next(context);
        }
        else
        {
            // User is not authenticated; return 401 Unauthorized.
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.Append("WWW-Authenticate", "Bearer");
            await context.Response.WriteAsync("Unauthorized");
        }
    }
}
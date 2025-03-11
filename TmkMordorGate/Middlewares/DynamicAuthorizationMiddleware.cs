using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Helpers;
using IMiddleware = TmkMordorGate.Middlewares.Interfaces.IMiddleware;

namespace TmkMordorGate.Middlewares;

public class DynamicAuthorizationMiddleware : IMiddleware
{
    private RequestDelegate? _next;
    private readonly IAuthorizationFactory _factory;

    public DynamicAuthorizationMiddleware(RequestDelegate next, IAuthorizationFactory factory)
    {
        _next = next;
        _factory = factory;
    }

    public RequestDelegate? Next
    {
        get => _next;
        set => _next = value;
    }

    public async Task Invoke(HttpContext context)
    {
        var authorizationService = _factory.GetAuthorizationService(context);
        if (authorizationService != null)
        {
            var authorized = await authorizationService.Authorize(context);
            if (authorized && _next != null)
            {
                await _next.Invoke(context);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Request is not authorized")
                    .ConfigureAwait(false);
                context.Abort();
            }
        }
        else
        {
            // Authorization service not found, so defaulting to not authorize by returning 403 Forbidden.
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden: Authorization service not found, defaulting to 403")
                .ConfigureAwait(false);
            context.Abort();
        }
    }
}
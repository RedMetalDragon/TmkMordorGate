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
            await authorizationService.Authorize(context).ConfigureAwait(false);
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
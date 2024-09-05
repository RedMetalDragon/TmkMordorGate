using System.Security;

namespace TmkMordorGate.Middlewares;

public class SetHeaderInGandalfService(RequestDelegate next) : ISetHeader
{
    private const string PathToCheck = "/api/v1/gandalf";

    public async Task SetHeader(HttpContext context, string key, string value)
    {
        if (context.Request.Path.Value != null && context.Request.Path.Value.Contains(PathToCheck))
        {
            context.Request.Headers.Append(key,value);
        }
        await next(context);
    }

    public RequestDelegate Next
    {
        get => next;
        set => throw new SecurityException("Next delegate can't be set from this middleware SetHeader");
    }

    public async Task Invoke(HttpContext context)
    {
        await SetHeader(context, "X-Tier", "Free");
    }
}
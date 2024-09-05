namespace TmkMordorGate.Middlewares;

public static class RequestSetHeaderInGandalfMiddleware
{
    public static IApplicationBuilder UseSetHeaderInGandalfMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SetHeaderInGandalfService>();
    }
}
using TmkMordorGate;
using TmkMordorGate.Config;
using Yarp.ReverseProxy.LoadBalancing;

//Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.ConfigureInitialServices();
var app = builder.Build();
app.TmkConfigureMiddleWares();
app.MapHealthChecks("/health");
app.MapControllers();
app.MapReverseProxy();
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request Path: {context.Request.Path}");
    await next.Invoke();
});
app.Run();
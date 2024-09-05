using TmkMordorGate;
using TmkMordorGate.Config;
using Yarp.ReverseProxy.LoadBalancing;

//Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.ConfigureInitialServices();
var app = builder.Build();
app.TmkConfigureMiddleWares();
app.MapHealthChecks("/health");
app.MapReverseProxy();
app.Run();
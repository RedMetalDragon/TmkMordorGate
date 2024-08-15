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

// Configure the HTTP request pipeline
//app.UseRateLimiter();
//app.UseHttpsRedirection();

//app.MapReverseProxy();
//InitialServicesConfig.ConfigurePipelineToSetupHeadersForGandalfService(app);
// app.MapGet("/debug/loadbalancer", (ILoadBalancingPolicy loadBalancer) =>
// {
//     if (loadBalancer is LoadBalancer customLoadBalancer)
//     {
//         return Results.Ok(customLoadBalancer.GetDebugInfo());
//     }
//     return Results.NotFound("Custom load balancer not found");
// });

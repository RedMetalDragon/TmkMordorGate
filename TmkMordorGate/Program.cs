using TmkMordorGate;

var builder = WebApplication.CreateBuilder(args);
InitialServicesConfig.ConfigureInitialServices(builder);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    // Register the rate limiter middleware

}

// Configure the HTTP request pipeline
app.UseRateLimiter();
app.UseHttpsRedirection();
app.MapHealthChecks("/health");
app.MapReverseProxy();
app.Run();

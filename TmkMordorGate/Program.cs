using TmkMordorGate;

var builder = WebApplication.CreateBuilder(args);
InitialServicesConfig.ConfigureInitialServices(builder);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    // Register the rate limiter middleware
    
}

// Configure the HTTP request pipeline.
app.UseRateLimiter();
app.UseHttpsRedirection();
app.MapReverseProxy();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;


namespace TmkMordorGate;
public static class InitialServicesConfig
{

    public static void ConfigureInitialServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks().AddCheck("basic", () => HealthCheckResult.Healthy("OK"));
        System.Console.WriteLine("Configuring services...");
        // Configure services based on the environment
        if (builder.Environment.IsDevelopment())
        {
            ConfigureDevelopmentServices(builder);
        }
        else
        {
            ConfigureProductionServices(builder);
        }

    }

    private static void ConfigureDevelopmentServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
        // Rate limiting configuration
        builder.Services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
            {
                options.Window = TimeSpan.FromSeconds(10);
                options.PermitLimit = 10;
            });
        });
        builder.Services.AddEndpointsApiExplorer();
    }

    private static void ConfigureProductionServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
        // Rate limiting configuration
        builder.Services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
            {
                options.Window = TimeSpan.FromSeconds(10);
                options.PermitLimit = 10;
            });
        });
        builder.Services.AddEndpointsApiExplorer();
    }

}

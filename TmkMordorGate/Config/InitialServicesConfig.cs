using Microsoft.AspNetCore.RateLimiting;

namespace TmkMordorGate;
public static class InitialServicesConfig
{

    public static void ConfigureInitialServices(this WebApplicationBuilder builder)
    {
        // Rate limiting configuration
        builder.Services.AddRateLimiter(rateLimiterOptions => {
            rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>{
                options.Window = TimeSpan.FromSeconds(10);
                options.PermitLimit = 10;
            });
        });
        builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
        builder.Services.AddEndpointsApiExplorer();
    }

}

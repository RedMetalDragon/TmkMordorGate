using System.Net;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TmkMordorGate.Services;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;
using DestinationConfig = Yarp.ReverseProxy.Configuration.DestinationConfig;

namespace TmkMordorGate.Config;

public static class InitialServicesConfig
{
    public static void ConfigureInitialServices(this WebApplicationBuilder builder)
    {
        
        builder.Services.AddHealthChecks()
            .AddCheck("basic", () => HealthCheckResult.Healthy("OK"));
        builder.Services.AddHttpClient();

        // Configure services based on the environment
        // ask for the value of the ASPNETCORE_ENVIRONMENT environment variable
        
        if (IsLocalDevelopmentRun(builder))
        {
            Console.WriteLine("Local development environment detected");
            builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
            ConfigureLocalServices(builder);
        }

        else if (builder.Environment.IsDevelopment())
        {
            ConfigureDevelopmentServices(builder);
            builder.Configuration.AddJsonFile("appsettings.Development.json", true, true);
        }

        else
        {
            builder.Configuration.AddJsonFile("appsettings.Staging.json", true, true);
            ConfigureProductionServices(builder);
        }
    }

    private static void ConfigureDevelopmentServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
        // Rate limiting configuration
        builder.Services.AddScoped<IMordorConfigurationService, MordorConfigurationService>();
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

    private static void ConfigureLocalServices(this WebApplicationBuilder builder)
    {
        // Load the reverse proxy configuration from the appsettings.Local.json file
        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        // Register the Mordor configuration service
        builder.Services.AddScoped<IMordorConfigurationService, MordorConfigurationService>();

        builder.Services.AddScoped<IMordorPickerDestinationsService, MordorConfigurationService>();

        builder.Services.AddSingleton<ILoadBalancingPolicy, LoadBalancer>();

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

    public static void TmkConfigureMiddleWares(this IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseHttpsRedirection();
        app.TmkConfigureHeadersForGandalfService();
    }

    #region Private

    private static void TmkConfigureHeadersForGandalfService(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapReverseProxy(proxyPipeline =>
            {
                proxyPipeline.Use(async (context, next) =>
                {
                    if (context.Request.Path.Value != null && context.Request.Path.Value.Contains("/api/v1/gandalf/"))
                    {
                        context.Request.Headers.Append("X-Tier", "Free");
                    }
                    await next();
                });
            });
        });
    }

    // This method is used to determine if the application is running in a local development environment
    private static bool IsLocalDevelopmentRun(this WebApplicationBuilder app)
    {
        var env = app.Environment;
        return env.EnvironmentName == "Local";
    }
    
    #endregion
}
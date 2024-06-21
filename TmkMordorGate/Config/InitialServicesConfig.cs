using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TmkMordorGate.Services;


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


        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            options.ListenAnyIP(5100, listenOptions =>
            {
                // Retrieve HTTPS configuration from MordorConfigurationService within the scope
                var configService = builder.Services.BuildServiceProvider().GetRequiredService<IMordorConfigurationService>();
                var certPath = configService.GetConfigurationValue("Certificate:Path");
                var keyPath = configService.GetConfigurationValue("Certificate:Key:Path");

                if (string.IsNullOrEmpty(certPath) || string.IsNullOrEmpty(keyPath))
                {
                    // Log exception using Serilog
                    throw new FieldAccessException("Invalid certPath or certPassword");
                }
                // Apply HTTPS configuration
                var cert = new X509Certificate2(certPath, keyPath);
                listenOptions.UseHttps(cert);
            });
        });
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

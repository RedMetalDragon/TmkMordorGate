using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.DbContext;
using TmkMordorGate.Helpers;
using TmkMordorGate.Middlewares;
using TmkMordorGate.Repositories;
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services;
using TmkMordorGate.Services.Interfaces;
using Yarp.ReverseProxy.LoadBalancing;
using TmkMordorGate.Services.Authorization;
namespace TmkMordorGate.Config;

public static class InitialServicesConfig
{
    public static void ConfigureInitialServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("basic", () => HealthCheckResult.Healthy("OK"));
        builder.Services.AddHttpClient();
        builder.Services.AddControllers()
            .AddJsonOptions(jsonOptions =>
            {
                jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

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
            builder.Configuration.AddJsonFile("appsettings.Development.json", true, true);
            Console.WriteLine("Development environment detected");
            ConfigureDevelopmentServices(builder);
        }

        else
        {
            builder.Configuration.AddJsonFile("appsettings.Staging.json", true, true);
            ConfigureProductionServices(builder);
        }
    }


    public static void TmkConfigureMiddleWares(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseMiddleware<CustomAuthenticationMiddleware>();
        app.UseSetHeaderInGandalfMiddleware();
        app.UseAuthorization();
    }

    #region Private

    // This method is used to determine if the application is running in a local development environment
    private static bool IsLocalDevelopmentRun(this WebApplicationBuilder app)
    {
        var env = app.Environment;
        return env.EnvironmentName == "Local";
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

    private static void ConfigureLocalServices(this WebApplicationBuilder builder)
    {
        // Load the reverse proxy configuration from the appsettings.Local.json file
        // and add the path prefix and request transform for the Gandalf service
        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        // Register the Mordor configuration service
        builder.Services.AddSingleton<IMordorConfigurationService, MordorConfigurationService>();
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
        // Register the Authentication configuration
        builder.Services.AddSingleton<IAuthenticationConfiguration, ConfigAuthentication>();
        var serviceProvider = builder.Services.BuildServiceProvider();
        var authConfig = serviceProvider.GetRequiredService<IAuthenticationConfiguration>();
        authConfig.ConfigureAuthentication(builder.Services, builder.Configuration);

        // Register the Database settings
        builder.Services.AddSingleton<IDatabaseSettings, TmkMySqlDatabaseSettings>();
        // Register the Database context
        builder.Services.AddDbContext<TimeKeeperDbContext>((serviceProvider, options) =>
        {
            var mordorConfigurationService = serviceProvider.GetRequiredService<IMordorConfigurationService>();
            var dbSettings = mordorConfigurationService.GetDatabaseSettings();
            options.UseMySql(
                TmkMySqlDatabaseSettings.FromJdbcUrl(dbSettings.Host, dbSettings.Username, dbSettings.Password)
                    .ConnectionString, new MySqlServerVersion(new Version(8, 0, 27)));
        });
        // Register the JWT Authorization
        builder.Services.AddAuthorizationBuilder().AddPolicy("Authenticated", policy =>
        {
            policy.RequireAuthenticatedUser(); // Requires valid JWT
        });
        // Register the Authentication repository
        builder.Services.AddSingleton<IAuthenticationRepository, TmkAccessControlRepository>();
        // Register the Authentication service
        builder.Services.AddSingleton<IAuthenticationService, TmkAuthenticationService>();
        
        // Register the Authorization services
        var authorizationFactory = new AuthorizationFactory();
        var authorizationInstancesFromJson = builder.Configuration.GetSection("AuthorizationInstances").GetChildren();
        foreach (var instance in authorizationInstancesFromJson)
        {
            var className = instance.GetValue<string>("Name");
            // Check if the class name is valid
            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentException("Class name cannot be null or empty", nameof(className));
            }
            if (!authorizationFactory.IsValidServiceType(className))
            {
                Console.WriteLine($"Invalid class name {className} cannot be used");
                continue;
            }
            var authService = authorizationFactory.CreateAuthorizationService(className);
            try
            {
                builder.Services.AddSingleton<IAuthorizationService>(authService);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating instance of {className}: {e.Message}");
                throw;
            }
        }
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
    
    #endregion
}

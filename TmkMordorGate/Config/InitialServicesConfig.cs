using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TmkMordorGate.DbContext;
using TmkMordorGate.Middlewares;
using TmkMordorGate.Services;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Transforms;

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
        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseMiddleware<CustomAuthenticationMiddleware>();
        app.UseSetHeaderInGandalfMiddleware();
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

        // Register the JWT Authentication
        builder.Services.AddAuthentication(schema =>
        {
            schema.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            schema.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            schema.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var jwtKey = builder.Configuration.GetValue<string>("JwtKey");
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration.GetValue<string>("JwtIssuer"), // Use IConfiguration directly
                ValidAudience = builder.Configuration.GetValue<string>("JwtAudience"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
            };
        });


        builder.Services.AddDbContext<TimeKeeperDbContext>((serviceProvider, options) =>
        {
            var mordorConfigurationService = serviceProvider.GetRequiredService<IMordorConfigurationService>();
            var dbSettings = mordorConfigurationService.GetDatabaseSettings();
            options.UseMySql(
                DatabaseSettings.FromJdbcUrl(dbSettings.Host, dbSettings.Username, dbSettings.Password)
                    .ConnectionString, new MySqlServerVersion(new Version(8, 0, 27)));
        });

        // Register the JWT Authorization
        builder.Services.AddAuthorizationBuilder().AddPolicy("JwtBearer", policy =>
        {
            policy.RequireAuthenticatedUser(); // Requires valid JWT
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

    #endregion
}
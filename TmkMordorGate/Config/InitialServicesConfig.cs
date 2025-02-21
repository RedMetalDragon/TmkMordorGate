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
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;

namespace TmkMordorGate.Config
{
    public static class ServiceConfigurationExtensions
    {
        public static void ConfigureInitialServices(this WebApplicationBuilder builder)
        {
            // Configure base services common to all environments
            builder.Services.AddBaseServices();

            // Select environment-specific configuration
            if (builder.Environment.EnvironmentName == "Local")
            {
                Console.WriteLine("Local development environment detected");
                builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
                LocalServicesConfig.Configure(builder);
            }
            else if (builder.Environment.IsDevelopment())
            {
                Console.WriteLine("Development environment detected");
                builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                DevelopmentServicesConfig.Configure(builder);
            }
            else
            {
                Console.WriteLine("Staging/Production environment detected");
                builder.Configuration.AddJsonFile("appsettings.Staging.json", optional: true, reloadOnChange: true);
                ProductionServicesConfig.Configure(builder);
            }
        }

        public static void ConfigureMiddlewares(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseMiddleware<CustomAuthenticationMiddleware>();
            app.UseSetHeaderInGandalfMiddleware();
            app.UseAuthorization();
        }
    }

    /// <summary>
    /// Base services common to all environments.
    /// </summary>
    public static class BaseServicesExtensions
    {
        public static void AddBaseServices(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck("basic", () => HealthCheckResult.Healthy("OK"));

            services.AddHttpClient();

            services.AddControllers().AddJsonOptions(jsonOptions =>
            {
                jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
        }

        public static void AddRateLimiterServices(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("fixed", limiterOptions =>
                {
                    limiterOptions.Window = TimeSpan.FromSeconds(10);
                    limiterOptions.PermitLimit = 10;
                });
            });
        }

        public static void AddReverseProxyServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddReverseProxy().LoadFromConfig(configuration.GetSection("ReverseProxy"));
        }
        
        public static void AddRedisCache(this IServiceCollection services, IMordorConfigurationService configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                var redisSettings = configuration.GetRedisCacheSettings();
                options.Configuration = $"{redisSettings.Host}:{redisSettings.Port},password={redisSettings.Password}";
                options.ConfigurationOptions = new ConfigurationOptions()
                {
                    Password = redisSettings.Password,
                    AbortOnConnectFail = true,
                    EndPoints = { options.Configuration }
                };
            });
        }
    }

    /// <summary>
    /// Services configuration for the Local environment.
    /// </summary>
    public static class LocalServicesConfig
    {
        public static void Configure(WebApplicationBuilder builder)
        {
            // Reverse proxy & rate limiter
            builder.Services.AddReverseProxyServices(builder.Configuration);
            builder.Services.AddRateLimiterServices();
            builder.Services.AddEndpointsApiExplorer();

            // Register Mordor configuration service
            builder.Services.AddSingleton<IMordorConfigurationService, MordorConfigurationService>();
            builder.Services.AddRedisCache(builder.Services.BuildServiceProvider().GetRequiredService<IMordorConfigurationService>());
            builder.Services.AddScoped<IMordorPickerDestinationsService, MordorConfigurationService>();
            builder.Services.AddSingleton<ILoadBalancingPolicy, LoadBalancer>();

            // Configure Authentication
            builder.Services.AddSingleton<IAuthenticationConfiguration, ConfigAuthentication>();
            BuildAndConfigureAuthentication(builder);

            // Database registration
            builder.Services.AddSingleton<IDatabaseSettings, TmkMySqlDatabaseSettings>();
            builder.Services.AddDbContext<TimeKeeperDbContext>((sp, options) =>
            {
                var mordorConfig = sp.GetRequiredService<IMordorConfigurationService>();
                var dbSettings = mordorConfig.GetDatabaseSettings();
                options.UseMySql(
                    TmkMySqlDatabaseSettings.FromJdbcUrl(dbSettings.Host, dbSettings.Username, dbSettings.Password).ConnectionString,
                    new MySqlServerVersion(new Version(8, 0, 27)));
            });

            // JWT Authorization
            builder.Services.AddAuthorizationBuilder().AddPolicy("Authenticated", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
            builder.Services.AddSingleton<IAuthenticationRepository, TmkAccessControlRepository>();
            builder.Services.AddSingleton<IAuthenticationService, TmkAuthenticationService>();
            // Register custom authorization services
            RegisterAuthorizationServices(builder);
        }

        private static void BuildAndConfigureAuthentication(WebApplicationBuilder builder)
        {
            using (var sp = builder.Services.BuildServiceProvider())
            {
                var authConfig = sp.GetRequiredService<IAuthenticationConfiguration>();
                authConfig.ConfigureAuthentication(builder.Services, builder.Configuration);
            }
        }

        private static void RegisterAuthorizationServices(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IAuthenticationAuthorizationRepository, TmkAccessControlRepository>();
            var authorizationFactory = new AuthorizationFactory(builder.Services.BuildServiceProvider());
            var authorizationInstances = builder.Configuration.GetSection("AuthorizationInstances").GetChildren();

            foreach (var instance in authorizationInstances)
            {
                var className = instance.GetValue<string>("Name");
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
    }

    /// <summary>
    /// Services configuration for the Development environment.
    /// </summary>
    public static class DevelopmentServicesConfig
    {
        public static void Configure(WebApplicationBuilder builder)
        {
            builder.Services.AddReverseProxyServices(builder.Configuration);
            builder.Services.AddScoped<IMordorConfigurationService, MordorConfigurationService>();
            builder.Services.AddRateLimiterServices();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSingleton<IMordorConfigurationService, MordorConfigurationService>();
            builder.Services.AddRedisCache(builder.Services.BuildServiceProvider().GetRequiredService<IMordorConfigurationService>());

            // Register custom authorization services (if needed)
            RegisterAuthorizationServices(builder);
        }

        private static void RegisterAuthorizationServices(WebApplicationBuilder builder)
        {
            var authorizationFactory = new AuthorizationFactory();
            var authorizationInstances = builder.Configuration.GetSection("AuthorizationInstances").GetChildren();

            foreach (var instance in authorizationInstances)
            {
                var className = instance.GetValue<string>("Name");
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
    }

    /// <summary>
    /// Services configuration for the Production environment.
    /// </summary>
    public static class ProductionServicesConfig
    {
        public static void Configure(WebApplicationBuilder builder)
        {
            builder.Services.AddReverseProxyServices(builder.Configuration);
            builder.Services.AddRateLimiterServices();
            builder.Services.AddEndpointsApiExplorer();

            // Register Mordor configuration & authentication services
            builder.Services.AddSingleton<IMordorConfigurationService, MordorConfigurationService>();
            builder.Services.AddSingleton<IAuthenticationConfiguration, ConfigAuthentication>();
            builder.Services.AddRedisCache(builder.Services.BuildServiceProvider().GetRequiredService<IMordorConfigurationService>());
            BuildAndConfigureAuthentication(builder);

            // Database registration
            builder.Services.AddSingleton<IDatabaseSettings, TmkMySqlDatabaseSettings>();
            builder.Services.AddDbContext<TimeKeeperDbContext>((sp, options) =>
            {
                var mordorConfig = sp.GetRequiredService<IMordorConfigurationService>();
                var dbSettings = mordorConfig.GetDatabaseSettings();
                options.UseMySql(
                    TmkMySqlDatabaseSettings.FromJdbcUrl(dbSettings.Host, dbSettings.Username, dbSettings.Password).ConnectionString,
                    new MySqlServerVersion(new Version(8, 0, 27)));
            });

            // JWT Authorization
            builder.Services.AddAuthorizationBuilder().AddPolicy("Authenticated", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
            builder.Services.AddSingleton<IAuthenticationRepository, TmkAccessControlRepository>();
            builder.Services.AddSingleton<IAuthenticationService, TmkAuthenticationService>();
            builder.Services.AddSingleton<IAuthenticationAuthorizationRepository, TmkAccessControlRepository>();

            // Register custom authorization services
            RegisterAuthorizationServices(builder);
        }

        private static void BuildAndConfigureAuthentication(WebApplicationBuilder builder)
        {
            using (var sp = builder.Services.BuildServiceProvider())
            {
                var authConfig = sp.GetRequiredService<IAuthenticationConfiguration>();
                authConfig.ConfigureAuthentication(builder.Services, builder.Configuration);
            }
        }

        private static void RegisterAuthorizationServices(WebApplicationBuilder builder)
        {
            var authorizationFactory = new AuthorizationFactory();
            var authorizationInstances = builder.Configuration.GetSection("AuthorizationInstances").GetChildren();

            foreach (var instance in authorizationInstances)
            {
                var className = instance.GetValue<string>("Name");
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
    }
}
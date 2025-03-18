using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.DbContext;
using TmkMordorGate.Helpers;
using TmkMordorGate.Repositories;
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services;
using TmkMordorGate.Services.Interfaces;
using Yarp.ReverseProxy.LoadBalancing;

namespace TmkMordorGate.Config;

public static class ConfigurationRunner
{
    public static void SetupLocalConfigurationPreBuild(WebApplicationBuilder builder)
    {
        var commonServicesInitialization = new CommonServicesInitialization(builder.Configuration);
        foreach (var commonServiceAction in commonServicesInitialization.BuildCommonServices())
        {
            commonServiceAction(builder.Services);
        }

        builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
        builder.Services.AddSingleton<IMordorConfigurationService, MordorConfigurationService>();
        builder.Services.AddSingleton<IMordorPickerDestinationsService, MordorConfigurationService>();
        builder.Services.AddSingleton<ILoadBalancingPolicy, LoadBalancer>();
        builder.Services.AddSingleton<IAuthenticationConfiguration, ConfigAuthentication>();
        builder.Services.AddSingleton<IDatabaseSettings, TmkMySqlDatabaseSettings>();
        builder.Services.AddScoped<IAuthenticationRepository, TmkAccessControlRepository>();
        builder.Services.AddScoped<IAuthenticationAuthorizationRepository, TmkAccessControlRepository>();
        builder.Services.AddScoped<IAuthenticationService, TmkAuthenticationService>();
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("Authenticated", policy => { policy.RequireAuthenticatedUser(); });
        builder.Services.AddSingleton<IAuthorizationFactory>(sp => new AuthorizationFactory(sp));
        builder.Services.AddEndpointsApiExplorer();
        var services = builder.Services.BuildServiceProvider();
        var authConfig = services.GetRequiredService<IAuthenticationConfiguration>();
        authConfig.ConfigureAuthentication(builder.Services, builder.Configuration);
        // ADD REDIS CACHE SERVICE TO THE CONTAINER
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            var mordorConfig = services.GetRequiredService<IMordorConfigurationService>();
            var redisSettings = mordorConfig.GetRedisCacheSettings();
            options.Configuration = $"{redisSettings.Host}:{redisSettings.Port},password={redisSettings.Password}";
            options.ConfigurationOptions = new ConfigurationOptions()
            {
                Password = redisSettings.Password,
                AbortOnConnectFail = true,
                EndPoints = { options.Configuration }
            };
        });

        // ADD DATABASE CONTEXT TO THE CONTAINER
        builder.Services.AddDbContext<TimeKeeperDbContext>((sp, options) =>
        {
            var mordorConfig = sp.GetRequiredService<IMordorConfigurationService>();
            var dbSettings = mordorConfig.GetDatabaseSettings();
            options.UseMySql(
                TmkMySqlDatabaseSettings.FromJdbcUrl(dbSettings.Host, dbSettings.Username, dbSettings.Password)
                    .ConnectionString,
                new MySqlServerVersion(new Version(8, 0, 27)));
        });
    }

    public static void SetupLocalConfigurationPostBuild(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var authorizationInstances = configuration.GetSection("AuthorizationInstances").GetChildren();
        var authorizationFactory = serviceProvider.GetRequiredService<IAuthorizationFactory>();
        // ADD AUTHORIZATION INSTANCES TO THE AUTHORIZATION FACTORY
        foreach (var instance in authorizationInstances)
        {
            var className = instance.GetValue<string>("Name");
            var targetRoute = instance.GetValue<string>("Route");
            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentException("Class name cannot be null or empty", nameof(className));
            }

            if (string.IsNullOrEmpty(targetRoute))
            {
                throw new ArgumentException("Target route cannot be null or empty", nameof(targetRoute));
            }

            try
            {
                var authorizationInstance =
                    authorizationFactory.CreateAuthorizationInstance((s => s.Length > 0), className, targetRoute);
                if (authorizationInstance == null)
                {
                    throw new ArgumentException("Authorization instance cannot be null", nameof(authorizationInstance));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public static void SetupDevelopmentConfiguration(WebApplicationBuilder builder)
    {
        // TODO: Implement
    }

    private static void SetupDevelopmentConfiguration(IServiceProvider services, WebApplicationBuilder builder)
    {
        // TODO: Implement
    }

    public static void SetupProductionConfiguration(WebApplicationBuilder builder)
    {
        // TODO: Implement
    }

    private static void SetupProductionConfiguration(IServiceProvider services, WebApplicationBuilder builder)
    {
        // TODO: Implement
    }
}
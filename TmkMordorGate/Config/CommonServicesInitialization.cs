using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services;

namespace TmkMordorGate.Config;

public class CommonServicesInitialization
{
    IConfiguration _configuration;

    public CommonServicesInitialization(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<Action<IServiceCollection>> BuildCommonServices()
    {
        return
        [
            AddHealthCustomized,
            AddControllersCustomized,
            AddHttpClientCustomized,
            AddRateLimiterServices,
            AddMemoryBlackListToken
        ];
    }

    private static void AddHealthCustomized(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHealthChecks()
            .AddCheck("basic", () => HealthCheckResult.Healthy("OK"));
    }

    private static void AddControllersCustomized(IServiceCollection serviceCollection)
    {
        serviceCollection.AddControllers().AddJsonOptions(jsonOptions =>
        {
            jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    }

    private static void AddHttpClientCustomized(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient();
    }

    private static void AddRateLimiterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("fixed", limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromSeconds(10);
                limiterOptions.PermitLimit = 10;
            });
        });
    }

    private static void AddMemoryBlackListToken(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IBlackListTokenService, MemoryBlackListToken>();
    }
}
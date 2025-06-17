using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Middlewares;
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services;
using TmkMordorGate.Services.Interfaces;
using TmkMordorGateTest.Mocks;

namespace TmkMordorGateTest.Setup;

public class RabbitMqTestSetup : IAsyncLifetime
{
    public TestServer Server { get; private set; }
    public HttpClient Client { get; private set; }
    public IServiceProvider ServiceProvider { get; private set; }
    public IConfiguration Configuration { get; private set; }

    public RabbitMqTestSetup()
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("test_settings_local.json", optional: true)
            .AddEnvironmentVariables();
        Configuration = configurationBuilder.Build();
    }

    public async Task InitializeAsync()
    {
        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                
                webBuilder.UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton<IConfiguration>(Configuration);
                        // Register fake dependencies for your middlewares.
                        services.AddSingleton<IMordorConfigurationService, FakeMordorConfigurationService>();
                        services
                            .AddSingleton<IAuthenticationAuthorizationRepository,
                                FakeAuthenticationAuthorizationRepository>();
                        services.AddSingleton<IAuthorizationFactory, FakeAuthorizationFactory>();
                        services.AddSingleton<IAuthenticationService, TmkAuthenticationService>();
                        // Register your custom middlewares.
                        services.AddTransient<DynamicAuthenticationMiddleware>();
                    })
                    .Configure(app => { app.UseMiddleware<DynamicAuthenticationMiddleware>(); });
            });
        var host = await hostBuilder.StartAsync();
        Server = host.GetTestServer();
        Client = Server.CreateClient();
        ServiceProvider = Server.Services;
    }
    
    public Task DisposeAsync() => Task.CompletedTask;
}
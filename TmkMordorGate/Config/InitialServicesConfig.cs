namespace TmkMordorGate;
public static class InitialServicesConfig
{

    public static void ConfigureInitialServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
        builder.Services.AddEndpointsApiExplorer();
    }

}

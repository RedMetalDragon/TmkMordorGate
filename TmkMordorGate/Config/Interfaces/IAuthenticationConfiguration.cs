using TmkMordorGate.Services;

namespace TmkMordorGate.Config.Interfaces;

public interface IAuthenticationConfiguration
{
    public void ConfigureAuthentication(IServiceCollection services, IConfiguration mordorConfigurationService);

    public void ConfigureAuthentication(IServiceCollection services);
}
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Services;

namespace TmkMordorGate.Config;

public class ConfigAuthentication: IAuthenticationConfiguration
{
    private readonly IMordorConfigurationService _mordorConfigurationService;

    public ConfigAuthentication(IMordorConfigurationService mordorConfigurationService)
    {
        _mordorConfigurationService = mordorConfigurationService;
    }
    
    public void ConfigureAuthentication(IServiceCollection services, IConfiguration mordorConfigurationService)
    {
        try
        {
            var jwtKey = _mordorConfigurationService.GetConfigurationValue("JwtKey");
            var jtwIssuer = _mordorConfigurationService.GetConfigurationValue("JwtIssuer");
            var jwtSingleAudience = _mordorConfigurationService.GetConfigurationValue("JwtAudience");
            // In production, we will have multiple audiences
            //var jwtAudiences = _mordorConfigurationService.GetArrayOfConfigurationValue("JwtAudience");
            services.AddAuthentication(schema =>
                {
                    schema.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    schema.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    schema.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
                        ValidateIssuer = true,
                        ValidIssuer = jtwIssuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSingleAudience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                    // TODO: Change to a logger service the Console.WriteLine
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
                            return Task.CompletedTask;
                        }
                    };
                });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void ConfigureAuthentication(IServiceCollection services)
    {
        throw new NotImplementedException();
    }
}
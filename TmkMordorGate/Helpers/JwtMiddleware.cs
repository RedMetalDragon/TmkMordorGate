using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TmkMordorGate.Services;

namespace TmkMordorGate.Helpers;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMordorConfigurationService _configurationService;

    public JwtMiddleware(RequestDelegate next, IMordorConfigurationService configurationService)
    {
        _next = next;
        _configurationService = configurationService;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
            AttachUserToContext(context, token);

        await _next(context);
    }

    private void AttachUserToContext(HttpContext context, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configurationService.GetConfigurationValue("JwtKey"));
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidIssuer = _configurationService.GetConfigurationValue("JwtIssuer"),
                ValidateAudience = true,
                ValidAudience = _configurationService.GetConfigurationValue("JwtAudience"),
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.First(x => x.Type == "id").Value;

            // Attach the user id to the context for further processing
            context.Items["UserId"] = userId;
        }
        catch
        {
            // Do nothing if the token validation fails
        }
    }
}
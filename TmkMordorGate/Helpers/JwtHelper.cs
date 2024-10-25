using TmkMordorGate.Models;
using TmkMordorGate.Services;

namespace TmkMordorGate.Helpers;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class JwtHelper(IMordorConfigurationService mordorConfigurationService)
{
    public string GenerateJwtToken(Auth authenticatedUser)
    {
        if (authenticatedUser == null)
            throw new ArgumentNullException(nameof(authenticatedUser), "Authenticated user cannot be null.");
        if (string.IsNullOrEmpty(authenticatedUser.Email))
            throw new ArgumentNullException(nameof(authenticatedUser.Email),
                "Authenticated user's email cannot be null.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(mordorConfigurationService.GetConfigurationValue("JwtKey"));
        var issuer = mordorConfigurationService.GetConfigurationValue("JwtIssuer");
        var audience = mordorConfigurationService.GetConfigurationValue("JwtAudience");
        //var tokenExpiryInHours = int.Parse(mordorConfigurationService.GetConfigurationValue("JwtExpiryInHours"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, authenticatedUser.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            //Expires = DateTime.UtcNow.AddHours(tokenExpiryInHours),
            Expires = DateTime.Now.AddHours(1),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
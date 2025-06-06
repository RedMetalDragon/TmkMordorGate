using System.Globalization;
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
        try
        {
            var key = Encoding.ASCII.GetBytes(mordorConfigurationService.GetConfigurationValue("JwtKey"));
            var issuer = mordorConfigurationService.GetConfigurationValue("JwtIssuer");
            var audience = mordorConfigurationService.GetConfigurationValue("JwtAudience");
            var tokenDescriptor = GenerateTokenDescriptor(authenticatedUser, key, issuer, audience);
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public SecurityTokenDescriptor GenerateTokenDescriptor(Auth authenticatedUser, byte[] key, string issuer,
        string audience)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, authenticatedUser.Email),
                new Claim(JwtRegisteredClaimNames.Jti, authenticatedUser.EmployeeID.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            NotBefore = DateTime.UtcNow,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        return tokenDescriptor;
    }
}
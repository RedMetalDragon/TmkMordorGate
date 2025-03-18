using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Helpers;
using TmkMordorGate.Models;
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Services;

public class TmkAuthenticationService(
    IAuthenticationAuthorizationRepository authenticationAuthorizationRepository,
    IMordorConfigurationService configurationService,
    IBlackListTokenService _blackListTokenService) : IAuthenticationService
{
    /// <summary>
    /// Authenticates a user based on the provided email and password.
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <param name="password">The password of the user.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the authentication process.
    /// Returns a <see cref="StatusCodeResult"/> with a status code of 400 if the email or password is invalid.
    /// Returns a <see cref="StatusCodeResult"/> with a status code of 500 if there is an internal server error.
    /// Returns a <see cref="StatusCodeResult"/> with a status code of 401 if the password is incorrect.
    /// Returns an <see cref="OkObjectResult"/> with an authenticated response if the authentication is successful.
    /// </returns>
    public async Task<IActionResult> Authenticate(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return new StatusCodeResult((int)HttpStatusCode.BadRequest);
        }

        if (!email.Contains('@'))
        {
            return new StatusCodeResult((int)HttpStatusCode.BadRequest);
        }

        var auth = await authenticationAuthorizationRepository.GetUser(email)!;

        if (auth == null)
        {
            return new StatusCodeResult((int)HttpStatusCode.NotAcceptable);
        }

        var employeeId = auth.EmployeeID;

        var isPasswordValid = BCryptHelper.VerifyPassword(password, auth.PasswordHash);

        if (!isPasswordValid)
            return new StatusCodeResult((int)HttpStatusCode.Unauthorized);

        var token = new JwtHelper(configurationService).GenerateJwtToken(auth);
        var authenticatedResponse = new AuthenticadedResponse(token, email, employeeId);
        return new OkObjectResult(authenticatedResponse);
    }

    public async Task<bool> IsAuthenticated(HttpContext context)
    {
        // Ensure an Authorization header exists
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            return false;
        // Extract the token from the Authorization header
        var tokenStr = authHeader.ToString().Replace("Bearer ", string.Empty);
        // Ensure the token is not null or empty
        if (string.IsNullOrWhiteSpace(tokenStr))
            return false;
        // Check if the token is blacklisted
        if (_blackListTokenService.IsBlacklisted(tokenStr))
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            // Validate token using token parameters obtained from configurationService
            var key = Encoding.ASCII.GetBytes(configurationService.GetConfigurationValue("JwtKey"));
            tokenHandler.ValidateToken(tokenStr, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            // Token validation failed
            return false;
        }
    }
}
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TmkMordorGate.Helpers;
using TmkMordorGate.Models;
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Services;

public class AuthenticationService(
    IAuthenticationRepository authenticationRepository,
    IMordorConfigurationService configurationService) : IAuthenticationService
{
    /// <summary>
    /// Authenticates a user.
    /// </summary>
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

        var auth = await authenticationRepository.GetUser(email)!;

        if (auth == null)
        {
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }

        var isPasswordValid = BCryptHelper.VerifyPassword(password, auth.PasswordHash);

        if (!isPasswordValid)
            return new StatusCodeResult((int)HttpStatusCode.Unauthorized);

        var token = new JwtHelper(configurationService).GenerateJwtToken(auth);
        var authenticatedResponse = new AuthenticadedResponse(token, email);
        return new OkObjectResult(authenticatedResponse);
    }
}
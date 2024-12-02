using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TmkMordorGate.Helpers;
using TmkMordorGate.Models;
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Services;

public class TmkAuthenticationService(
    IAuthenticationRepository authenticationRepository,
    IMordorConfigurationService configurationService) : IAuthenticationService
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

        var auth = await authenticationRepository.GetUser(email)!;

        if (auth == null)
        {
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }

        var employeeId = auth.EmployeeID;

        var isPasswordValid = BCryptHelper.VerifyPassword(password, auth.PasswordHash);

        if (!isPasswordValid)
            return new StatusCodeResult((int)HttpStatusCode.Unauthorized);

        var token = new JwtHelper(configurationService).GenerateJwtToken(auth);
        var authenticatedResponse = new AuthenticadedResponse(token, email, employeeId);
        return new OkObjectResult(authenticatedResponse);
    }
}
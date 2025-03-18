using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TmkMordorGate.Config;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Helpers;
using TmkMordorGate.Models;
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services;
using TmkMordorGate.Services.Authorization;
using TmkMordorGate.Services.Interfaces;
using IAuthenticationService = TmkMordorGate.Services.Interfaces.IAuthenticationService;

namespace TmkMordorGateTest.Mocks;

public class FakeAccessControlRepository : IAuthenticationAuthorizationRepository
{
    // Explicit implementation for IAuthenticationRepository.GetUser using emailAddress
    Task<Auth?> IAuthenticationRepository.GetUser(string emailAddress)
    {
        // For testing, return a valid Auth if emailAddress is "test@example.com".
        if (emailAddress == "valid96@gmail.com")
        {
            return Task.FromResult<Auth?>(new Auth { AuthID = 123, Email = "valid96@gmail.com" });
        }

        return Task.FromResult<Auth?>(null);
    }

    public Task<IEnumerable<string>> GetPermissions(int roleId)
    {
        // For testing, if roleId is 1, return a sample permission list; otherwise, return an empty list.
        if (roleId == 1)
        {
            return Task.FromResult<IEnumerable<string>>(new List<string> { "read", "write" });
        }

        return Task.FromResult<IEnumerable<string>>(new List<string>());
    }

    public Task<Feature?> GetFeature(int featureId)
    {
        // For testing, if featureId is 1, return a dummy Feature; otherwise, return null.
        if (featureId == 1)
        {
            return Task.FromResult<Feature?>(new Feature { FeatureID = 1, FeatureName = "FeatureOne" });
        }

        return Task.FromResult<Feature?>(null);
    }

    public Task<Plan?> GetPlan(int planId)
    {
        // For testing, if planId is 1, return a dummy Plan; otherwise, return null.
        if (planId == 1)
        {
            return Task.FromResult<Plan?>(new Plan { PlanID = 1, PlanName = "PlanBasic" });
        }

        return Task.FromResult<Plan?>(null);
    }

    public Task<Role?> GetRole(int roleId)
    {
        // For testing, if roleId is 1, return a dummy Role; otherwise, return null.
        if (roleId == 1)
        {
            return Task.FromResult<Role?>(new Role { RoleID = 1, RoleName = "Admin" });
        }

        return Task.FromResult<Role?>(null);
    }
}

public class FakeAuthorizationFactory : IAuthorizationFactory
{
    private IServiceProvider _serviceProvider;

    public FakeAuthorizationFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IAuthorizationService? CreateAuthorizationInstance(string className)
    {
        switch (className)
        {
            case "TmkBrain":
                var iAuthenticationAuthorizationRepository =
                    _serviceProvider.GetService<IAuthenticationAuthorizationRepository>();
                return new TmkBrain(iAuthenticationAuthorizationRepository);
            default:
                return new Dummy();
        }
    }

    public IAuthorizationService? CreateAuthorizationInstance(Func<string, bool> predicate, string className)
    {
        return CreateAuthorizationInstance(className);
    }

    public IAuthorizationService? CreateAuthorizationInstance(Func<string, bool> predicate, string className,
        string targetRoute)
    {
        return CreateAuthorizationInstance(className);
    }

    public IAuthorizationService? GetAuthorizationService(HttpContext context)
    {
        throw new NotImplementedException();
    }
}

public class FakeMordorConfigurationService : IMordorConfigurationService
{
    public string GetConfigurationValue(string key)
    {
        return key switch
        {
            "JwtKey" => "122340SJubaasInvalid^4hYKeyYs6tb316MasKeep:'/<",
            "JwtIssuer" => "issuer",
            "JwtAudience" => "audience",
            _ => "test"
        };
    }

    public IEnumerable<string> GetArrayOfConfigurationValue(string arrayKeyPrefix)
    {
        return ["TEST"];
    }

    public IEnumerable<string> GetAllKeys()
    {
        return ["TEST"];
    }

    public IDatabaseSettings GetDatabaseSettings()
    {
        return new TmkMySqlDatabaseSettings
        {
            Host = "localhost:3306",
            Username = "test",
            Password = "test"
        };
    }

    public IRedisCacheSettings GetRedisCacheSettings()
    {
        return new TmkRedisCacheSettings
        {
            Host = "localhost",
            Port = "6379",
            Password = "test",
        };
    }
}

public class FakeAuthenticationService : IAuthenticationService
{
    private IAuthenticationRepository _authenticationRepository;
    private IMordorConfigurationService _mordorConfigurationService;

    public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
    {
        throw new NotImplementedException();
    }

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

        var auth = await _authenticationRepository.GetUser(email)!;

        if (auth == null)
        {
            return new StatusCodeResult((int)HttpStatusCode.NotAcceptable);
        }

        var employeeId = auth.EmployeeID;

        var isPasswordValid = BCryptHelper.VerifyPassword(password, auth.PasswordHash);

        if (!isPasswordValid)
            return new StatusCodeResult((int)HttpStatusCode.Unauthorized);

        var token = new JwtHelper(_mordorConfigurationService).GenerateJwtToken(auth);
        var authenticatedResponse = new AuthenticadedResponse(token, email, employeeId);
        return new OkObjectResult(authenticatedResponse);
    }

    public async Task<bool> IsAuthenticated(HttpContext context)
    {
        // Ensure an Authorization header exists
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            return false;

        var tokenStr = authHeader.ToString().Replace("Bearer ", string.Empty);
        if (string.IsNullOrWhiteSpace(tokenStr))
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            // Validate token using token parameters obtained from configurationService
            var key = Encoding.ASCII.GetBytes(_mordorConfigurationService.GetConfigurationValue("JwtKey"));
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

public class FakeAuthenticationAuthorizationRepository : IAuthenticationAuthorizationRepository
{
    public FakeAuthenticationAuthorizationRepository()
    {
    }

    public Task<Auth?>? GetUser(string emailAddress)
    {
        if (emailAddress != "valid96@gmail.com") 
            return Task.FromResult<Auth?>(null);
        var validAuth = new Auth
        {
            AuthID = 1,
            Email = emailAddress
        };
        return Task.FromResult<Auth?>(validAuth);

    }

    public Task<IEnumerable<string>> GetPermissions(int roleId)
    {
        throw new NotImplementedException();
    }

    public Task<Feature?> GetFeature(int featureId)
    {
        throw new NotImplementedException();
    }

    public Task<Plan?> GetPlan(int planId)
    {
        throw new NotImplementedException();
    }

    public Task<Role?> GetRole(int roleId)
    {
        throw new NotImplementedException();
    }
}

public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // For testing, you can simulate different scenarios:
        // Return a successful authentication with a dummy ClaimsPrincipal
        var claims = new[] { new System.Security.Claims.Claim("TestClaim", "true") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestScheme");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
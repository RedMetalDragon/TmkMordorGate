using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Helpers;
using TmkMordorGate.Middlewares;
using TmkMordorGate.Models;
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services;
using TmkMordorGateTest.Setup;

namespace TmkMordorGateTest.IntegrationTests;

public class DynamicAuthenticationMiddlewareTests : IClassFixture<MiddlewareTestFixture>
{
    private MiddlewareTestFixture _fixture;

    public DynamicAuthenticationMiddlewareTests()
    {
        _fixture = new MiddlewareTestFixture();
        _fixture.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Authorize_Login_Request_Access()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/mordor/login")
        {
            Content = new StringContent(
                "{\"email_address\":\"rxxxxx\",\"password\":\"xxxxxxxx\"}",
                Encoding.UTF8,
                "application/json")
        };
        // Act
        var response = await _fixture.Client.SendAsync(request).WaitAsync(TimeSpan.FromSeconds(5));
        // Assert
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Disallow_Core_Request_Unauthorized()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/mordor/core");
        // Act
        var response = await _fixture.Client.SendAsync(request).WaitAsync(TimeSpan.FromSeconds(5));
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Disallow_Request_With_Expired_Token()
    {
        var auth = new Auth
        {
            Email = "rxxxxx",
            EmployeeID = 123
        };
        var configValues = _fixture.ServiceProvider.GetService<IMordorConfigurationService>();
        var key = Encoding.ASCII.GetBytes(configValues.GetConfigurationValue("JwtKey"));
        var audience = configValues.GetConfigurationValue("JwtAudience");
        var issuer = configValues.GetConfigurationValue("JwtIssuer");
        var tokenDesciptor =
            new JwtHelper(_fixture.ServiceProvider.GetService<IMordorConfigurationService>() ?? throw new Exception(""))
                .GenerateTokenDescriptor(auth, key, issuer, audience);
        tokenDesciptor.NotBefore = DateTime.UtcNow.AddMinutes(-10);
        tokenDesciptor.Expires = DateTime.UtcNow.AddMinutes(-5);
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDesciptor);
        var tokenString = tokenHandler.WriteToken(token);
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/mordor/core");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
        // Act
        var response = await _fixture.Client.SendAsync(request).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
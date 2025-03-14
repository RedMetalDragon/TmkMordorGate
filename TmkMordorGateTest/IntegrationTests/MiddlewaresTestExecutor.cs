using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGateTest.Setup;

namespace TmkMordorGateTest.IntegrationTests;

public class TmkMiddlewaresTests : IAsyncLifetime
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private const string SecretKey = "supersecretkey!123NeedsTo98BeGreater"; // For testing purposes only

        public TmkMiddlewaresTests()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseTestServer()
                    .ConfigureServices(services =>
                    {
                        // Configure Authentication with JWT Bearer
                        services.AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        })
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = false,
                                ValidateAudience = false,
                                ValidateLifetime = false,
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey))
                            };

                            options.Events = new JwtBearerEvents
                            {
                                OnTokenValidated = context => Task.CompletedTask
                            };
                        });

                        // Register the fake access control repository
                        services.AddScoped<IAuthenticationAuthorizationRepository, FakeAccessControlRepository>();
                        services.AddScoped<IAuthorizationFactory, AuthorizationFactory>();
                    })
                    .Configure(app =>
                    {
                        app.UseAuthentication();
                        app.UseMiddleware<DynamicAuthenticationMiddleware>();
                        app.UseMiddleware<DynamicAuthorizationMiddleware>();

                        // Terminal middleware returns OK if reached
                        app.Run(async context =>
                        {
                            await context.Response.WriteAsync("OK");
                        });
                    });
                });

            var host = hostBuilder.Start();
            _server = host.GetTestServer();
            _client = _server.CreateClient();
        }

        // Helper method to create a JWT token with specified claims
        private string CreateJwtToken(params Claim[] claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [Fact]
        public async Task ValidToken_WithBrainAccess_ReturnsOK()
        {
            // Arrange: Create a token with BrainAccess claim set to true
            var token = CreateJwtToken(new Claim("BrainAccess", "true"), new Claim("sub", "123"));
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act: Request a brain-protected endpoint
            var response = await _client.GetAsync("/api/v1/brain/users/6");

            // Assert: Should pass through the middleware and return OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ValidToken_WithoutBrainAccess_ReturnsUnauthorized()
        {
            // Arrange: Create a token with BrainAccess claim set to false
            var token = CreateJwtToken(new Claim("BrainAccess", "false"), new Claim("sub", "123"));
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act: Request a brain-protected endpoint
            var response = await _client.GetAsync("/api/v1/brain/users/6");

            // Assert: Should be unauthorized
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task NoToken_ReturnsUnauthorized()
        {
            // Arrange: Remove any authorization header
            _client.DefaultRequestHeaders.Authorization = null;

            // Act: Request a brain-protected endpoint
            var response = await _client.GetAsync("/api/v1/brain/users/6");

            // Assert: Should be unauthorized
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task DisposeAsync() => Task.CompletedTask;
    }
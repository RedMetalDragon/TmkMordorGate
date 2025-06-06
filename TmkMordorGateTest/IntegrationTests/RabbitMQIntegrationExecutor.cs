using System.Net;
using TmkMordorGateTest.Setup;

namespace TmkMordorGateTest.IntegrationTests;

public class RabbitMqIntegrationExecutor: IClassFixture<RabbitMqTestSetup>, IAsyncDisposable
{
    private readonly RabbitMqTestSetup _fixture;

    public RabbitMqIntegrationExecutor(RabbitMqTestSetup fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task RateLimiter_ShouldReturnTooManyRequests_WhenThresholdExceeded()
    {
        {
            // Arrange
            // Adjust the endpoint as appropriate for your application.
            var endpoint = "/";
            // Adjust allowedRequestCount based on your configured rate limit.
            var allowedRequestCount = 5;
            HttpResponseMessage response = null;

            // Act: send allowed number of requests that should pass
            for (int i = 0; i < allowedRequestCount; i++)
            {
                response = await _client.GetAsync(endpoint);
                // These must not be blocked
                Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
            }

            // Act: a next request should be rate-limited
            response = await _client.GetAsync(endpoint);

            // Assert that the rate limiter is working and returns 429 status code.
            Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        }
    }
    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }
}
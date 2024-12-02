using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
namespace TmkMordorGateTest.Mocks;


public class MockProxyConfigProvider : IProxyConfigProvider
{
    private readonly IProxyConfig _config;

    public MockProxyConfigProvider()
    {
        _config = new MockProxyConfig();
    }

    public IProxyConfig GetConfig()
    {
        return _config;
    }
}

public class MockProxyConfig : IProxyConfig
{
    public IReadOnlyList<RouteConfig> Routes { get; } = new List<RouteConfig>()
    {
        new RouteConfig
        {
            RouteId = "route1",
            Match = new RouteMatch
            {
                Path = "/api/{**catch-all}"
            },
            ClusterId = "cluster1"
        }
    };

    public IReadOnlyList<ClusterConfig> Clusters { get; } = new List<ClusterConfig>()
    {
        new ClusterConfig
        {
            ClusterId = "cluster1",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "destination1", new DestinationConfig { Address = "https://localhost:5001" } }
            }
        }
    };

    public IChangeToken ChangeToken { get; } = new CancellationChangeToken(new CancellationToken());
}
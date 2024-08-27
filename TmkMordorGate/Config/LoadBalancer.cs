using Yarp.ReverseProxy.Model;
using Yarp.ReverseProxy.LoadBalancing;
using System.Net;
using TmkMordorGate.Services;
using Yarp.ReverseProxy.Configuration;

namespace TmkMordorGate.Config
{
    public class LoadBalancer : ILoadBalancingPolicy
    {
        private readonly ILogger<LoadBalancer> _logger;
        private readonly HttpMessageInvoker _httpClient;
        private readonly IMordorPickerDestinationsService _mordorPickerDestinationsService;
        private readonly IDictionary<string, IList<(string, DestinationConfig)>> _availableDestinations;

        public LoadBalancer(ILogger<LoadBalancer> logger,
            IMordorPickerDestinationsService mordorPickerDestinationsService)
        {
            _logger = logger;
            _httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false
            });
            _mordorPickerDestinationsService = mordorPickerDestinationsService;
            _availableDestinations = mordorPickerDestinationsService.GetAvailableDestinationConfigs();
        }

        public string Name => "LocalOrDocker";

        /// <summary>
        /// The function `PickDestination` selects a destination state based on the provided context,
        /// cluster state, and available destinations.
        /// </summary>
        /// /// <param name="context">The `HttpContext` parameter typically represents the current HTTP
        /// request context in ASP.NET applications. It provides access to information about the
        /// incoming HTTP request, such as headers, cookies, and query parameters.</param>
        /// <param name="cluster">ClusterState represents the current state of a cluster in the
        /// system. It contains information about the cluster such as ClusterId and other relevant
        /// details.</param>
        /// <param name="availableDestinations">The `availableDestinations` parameter is a list of
        /// `DestinationState` objects representing the available destinations that can be chosen
        /// from.</param>
        /// <returns>
        /// The method is returning a nullable `DestinationState` object.
        /// </returns>
        public DestinationState? PickDestination(HttpContext context, ClusterState cluster,
            IReadOnlyList<DestinationState> availableDestinations)
        {
            if (_availableDestinations.TryGetValue(cluster.ClusterId, out var destinationConfigs))
            {
                return availableDestinations.FirstOrDefault(destination =>
                    destinationConfigs.Any(config => config.Item1 == destination.DestinationId));
            }
            throw new ApplicationException("Not instance to redirected found");
        }
    }
}
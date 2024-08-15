using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.LoadBalancing;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using DestinationConfig = Yarp.ReverseProxy.Configuration.DestinationConfig;

namespace TmkMordorGate.Services;

public interface IMordorConfigurationService
{
    string GetConfigurationValue(string key);
}

public interface IMordorPickerDestinationsService
{
    IDictionary<string, IList<(string, DestinationConfig)>> GetAvailableDestinationConfigs();
    bool CheckDestinationAvailability(DestinationConfig destinationConfig);
}

public class MordorConfigurationService : IMordorConfigurationService, IMordorPickerDestinationsService
{
    private readonly IProxyConfigProvider _proxyConfigProvider;
    private readonly IConfiguration _configuration;
    private readonly HttpMessageInvoker _httpClient;

    public MordorConfigurationService(IConfiguration configuration, IProxyConfigProvider proxyConfigProvider)
    {
        _configuration = configuration;
        _proxyConfigProvider = proxyConfigProvider;
        _httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false
        });
    }

    /// <summary>
    ///     The GetConfigurationValue function retrieves a configuration value by key, replacing ':'
    ///     with '_' for environment variables if necessary.
    /// </summary>
    /// <param name="key">
    ///     The `GetConfigurationValue` method takes a `key` parameter, which is used
    ///     to retrieve a configuration value from the `_configuration` dictionary. If the value is not
    ///     found in the dictionary, it attempts to retrieve the value from an environment variable by
    ///     replacing any ':' characters in the key with '_' and
    /// </param>
    /// <returns>
    ///     The method `GetConfigurationValue` returns the configuration value associated with the
    ///     provided key. If the value is not found in the configuration, it attempts to retrieve it
    ///     from environment variables by replacing ':' with '_' in the key and converting it to
    ///     uppercase. If the value is still not found, it throws an `InvalidOperationException` with a
    ///     message indicating that the configuration value for the key was not found.
    /// </returns>
    public string GetConfigurationValue(string key)
    {
        var value = _configuration[key];

        if (!string.IsNullOrEmpty(value))
            return value ?? throw new InvalidOperationException($"Configuration value for key '{key}' not found.");

        // Replace ':' with '_' for environment variables
        var listOf = Environment.GetEnvironmentVariables();
        value = Environment.GetEnvironmentVariable(key.Replace(':', '_').ToUpper());

        return value ?? throw new InvalidOperationException($"Configuration value for key '{key}' not found.");
    }

    /// <summary>
    /// This C# function retrieves available destination configurations for each cluster from a clusters
    /// collection.
    /// </summary>
    /// <returns>
    /// The method `GetAvailableDestinationConfigs` returns a dictionary where the key is a string
    /// representing a cluster ID, and the value is a list of tuples. Each tuple contains a string
    /// representing a destination key and a `DestinationConfig` object.
    /// </returns>
    public IDictionary<string, IList<(string, DestinationConfig)>> GetAvailableDestinationConfigs()
    {
        var clustersCollection = _proxyConfigProvider.GetConfig();
        var availableDestinations = new Dictionary<string, IList<(string, DestinationConfig)>>();
        foreach (var clusterConfig in clustersCollection.Clusters)
        {
            if (clusterConfig.Destinations == null)
            {
                continue;
            }

            var currentClusterId = clusterConfig.ClusterId;
            var listOfDestinations = new List<(string, DestinationConfig)>();
            availableDestinations.Add(currentClusterId, listOfDestinations);

            foreach (var destinationConfig in clusterConfig.Destinations)
            {
                if (CheckDestinationAvailability(destinationConfig.Value))
                {
                    availableDestinations[currentClusterId]
                        .Add(new ValueTuple<string, DestinationConfig>(destinationConfig.Key, destinationConfig.Value));
                }
            }
        }
        return availableDestinations;
    }

    /// <summary>
    /// The function `CheckDestinationAvailability` sends a GET request to a specified destination
    /// address and returns true if the response indicates success or not found, otherwise false.
    /// </summary>
    /// <param name="destinationConfig">DestinationConfig is a class or data structure that contains
    /// information about a destination, such as its address or URL. In the provided code snippet, the
    /// CheckDestinationAvailability method takes a DestinationConfig object as a parameter to check the
    /// availability of the destination by sending an HTTP GET request to the specified address and
    /// checking</param>
    /// <returns>
    /// The method `CheckDestinationAvailability` returns a boolean value indicating whether the
    /// destination specified in the `DestinationConfig` is available. It returns `true` if the HTTP
    /// response is successful (status code 2xx) or if the status code is `NotFound` (404), and `false`
    /// if there is an exception during the request or the response is not successful.
    /// </returns>
    public bool CheckDestinationAvailability(DestinationConfig destinationConfig)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(destinationConfig.Address));
            var response = _httpClient.SendAsync(request, new CancellationToken()).Result;
            var debugValue = response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound;
            return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
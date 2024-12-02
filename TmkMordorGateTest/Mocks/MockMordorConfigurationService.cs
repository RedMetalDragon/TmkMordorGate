using Microsoft.Extensions.Configuration;
using TmkMordorGate.Config;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Services;

namespace TmkMordorGateTest.Mocks;

public class MockMordorConfigurationService : IMordorConfigurationService
{
    private readonly IConfiguration _configuration;

    public MockMordorConfigurationService(IConfiguration config)
    {
        _configuration = config;
    }
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

    public IEnumerable<string> GetArrayOfConfigurationValue(string arrayKeyPrefix)
    {
        var allConfigKeys = GetAllKeys();
        var arrayKeys = allConfigKeys.Where(x => x.StartsWith(arrayKeyPrefix));
        return arrayKeys.Select(GetConfigurationValue);
    }

    public IEnumerable<string> GetAllKeys()
    {
        return _configuration.AsEnumerable().Select(x => x.Key);
    }

    public IDatabaseSettings GetDatabaseSettings()
    {
        throw new NotImplementedException();
    }
}
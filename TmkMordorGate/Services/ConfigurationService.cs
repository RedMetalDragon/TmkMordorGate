namespace TmkMordorGate.Services
{
    public interface IMordorConfigurationService
    {
        string GetConfigurationValue(string key);
    }

    public class MordorConfigurationService : IMordorConfigurationService
    {
        private readonly IConfiguration _configuration;

        public MordorConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// The GetConfigurationValue function retrieves a configuration value by key, replacing ':'
        /// with '_' for environment variables if necessary.
        /// </summary>
        /// <param name="key">The `GetConfigurationValue` method takes a `key` parameter, which is used
        /// to retrieve a configuration value from the `_configuration` dictionary. If the value is not
        /// found in the dictionary, it attempts to retrieve the value from an environment variable by
        /// replacing any ':' characters in the key with '_' and</param>
        /// <returns>
        /// The method `GetConfigurationValue` returns the configuration value associated with the
        /// provided key. If the value is not found in the configuration, it attempts to retrieve it
        /// from environment variables by replacing ':' with '_' in the key and converting it to
        /// uppercase. If the value is still not found, it throws an `InvalidOperationException` with a
        /// message indicating that the configuration value for the key was not found.
        /// </returns>
        public string GetConfigurationValue(string key)
        {
            string? value = _configuration[key];

            if (string.IsNullOrEmpty(value))
            {
                // Replace ':' with '_' for environment variables
                var listOf = Environment.GetEnvironmentVariables();
                value = Environment.GetEnvironmentVariable(key.Replace(':', '_').ToUpper());
            }

            return value ?? throw new InvalidOperationException($"Configuration value for key '{key}' not found.");
        }

    }
}

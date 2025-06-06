using TmkMordorGate.Config.Interfaces;

namespace TmkMordorGate.Config;

public class TmkRedisCacheSettings : IRedisCacheSettings
{
    public required string Port { get; set; }
    public required string Host { get; set; }
    public required string Password { get; set; }
}
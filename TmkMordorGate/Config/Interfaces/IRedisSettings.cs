namespace TmkMordorGate.Config.Interfaces;

public interface IRedisCacheSettings
{
    string Port { get; set; }
    string Host { get; set; }
    string Password { get; set; }
}
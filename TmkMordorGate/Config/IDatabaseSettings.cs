namespace TmkMordorGate.Config;

public interface IDatabaseSettings
{
    string ConnectionString { get; }
    string DatabaseName { get; set; }
    string Port { get; set; }
    string Host { get; set; }
    string Username { get; set; }
    string Password { get; set; }
}
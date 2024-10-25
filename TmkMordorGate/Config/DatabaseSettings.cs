namespace TmkMordorGate.Config;

public class DatabaseSettings
{
    public string ConnectionString => $"Server={Host};Database={DatabaseName};User={Username};Password={Password};Port={Port};";

    public static DatabaseSettings FromJdbcUrl(string jdbcUrl, string username, string password)
    {
        var uri = new Uri(jdbcUrl.Replace("jdbc:mysql://", "http://"));
        return new DatabaseSettings
        {
            Host = uri.Host,
            DatabaseName = uri.AbsolutePath.Trim('/'),
            Username = username,
            Password = password,
            Port = uri.Port.ToString()
        };
    }
    public string DatabaseName { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
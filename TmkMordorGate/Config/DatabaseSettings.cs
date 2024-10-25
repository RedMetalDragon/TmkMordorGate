namespace TmkMordorGate.Config;

public class DatabaseSettings
{
    public string ConnectionString =>
        $"Server={Host};Database={DatabaseName};User={Username};Password={Password};Port={Port};";

    /// <summary>
    /// Creates a new instance of <see cref="DatabaseSettings"/> from a JDBC URL.
    /// </summary>
    /// <param name="jdbcUrl">The JDBC URL to parse.</param>
    /// <param name="username">The username for the database connection.</param>
    /// <param name="password">The password for the database connection.</param>
    /// <returns>A new instance of <see cref="DatabaseSettings"/> populated with the parsed values.</returns>
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
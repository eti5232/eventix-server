using System.Globalization;
using Npgsql;

namespace Eventix.Infrastructure;

/// <summary>
/// Hosting platforms such as Render give the database address as a URL (<c>postgres://user:pass@host:5432/db</c>),
/// but Npgsql expects <c>Host=...;Username=...</c>. This converts the URL form and leaves the normal form untouched.
/// </summary>
public static class PostgresConnectionString
{
    /// <summary>Returns a connection string Npgsql understands.</summary>
    public static string Normalize(string connectionString)
    {
        var isUrl = connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
                    || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);
        if (!isUrl)
            return connectionString;

        var uri = new Uri(connectionString);
        var credentials = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : null,
            // The platform's internal address needs no TLS, its public address does; "Prefer" works for both.
            SslMode = ReadSslMode(uri.Query)
        };
        return builder.ConnectionString;
    }

    private static SslMode ReadSslMode(string query)
    {
        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase) && parts.Length == 2
                && Enum.TryParse<SslMode>(parts[1].Replace("-", string.Empty, StringComparison.Ordinal), ignoreCase: true, out var mode))
            {
                return mode;
            }
        }
        return SslMode.Prefer;
    }
}

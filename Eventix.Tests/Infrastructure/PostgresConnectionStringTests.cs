using Eventix.Infrastructure;
using Npgsql;

namespace Eventix.Tests.Infrastructure;

public class PostgresConnectionStringTests
{
    [Fact]
    public void Normalize_ConvertsPostgresUrlToNpgsqlForm()
    {
        var result = new NpgsqlConnectionStringBuilder(
            PostgresConnectionString.Normalize("postgres://eventix_user:p%40ss%3Aword@db-host:5433/eventix_db"));

        Assert.Equal("db-host", result.Host);
        Assert.Equal(5433, result.Port);
        Assert.Equal("eventix_db", result.Database);
        Assert.Equal("eventix_user", result.Username);
        Assert.Equal("p@ss:word", result.Password); // percent-encoded characters are decoded
        Assert.Equal(SslMode.Prefer, result.SslMode);
    }

    [Fact]
    public void Normalize_UsesDefaultPortAndHonoursSslModeFromTheUrl()
    {
        var result = new NpgsqlConnectionStringBuilder(
            PostgresConnectionString.Normalize("postgresql://u:p@db-host/eventix_db?sslmode=require"));

        Assert.Equal(5432, result.Port);
        Assert.Equal(SslMode.Require, result.SslMode);
    }

    [Fact]
    public void Normalize_LeavesTheNormalFormUntouched()
    {
        const string normal = "Host=localhost;Port=5432;Database=EventixDb;Username=postgres;Password=secret";

        Assert.Equal(normal, PostgresConnectionString.Normalize(normal));
    }
}

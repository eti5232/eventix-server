namespace Eventix.Application.Options;

/// <summary>
/// JWT settings, bound from the "Jwt" configuration section.
/// <see cref="Key"/> is a secret: it is set only through user-secrets (or an environment variable), never in appsettings.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>Signing key. At least 32 bytes are required for HS256.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Token lifetime in minutes.</summary>
    public int ExpiryMinutes { get; set; } = 60;
}

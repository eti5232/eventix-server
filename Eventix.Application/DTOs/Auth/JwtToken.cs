namespace Eventix.Application.DTOs.Auth;

/// <summary>A generated JWT and the moment it expires (UTC).</summary>
public record JwtToken(string Value, DateTime ExpiresAtUtc);

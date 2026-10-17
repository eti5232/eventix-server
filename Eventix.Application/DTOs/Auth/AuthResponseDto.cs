namespace Eventix.Application.DTOs.Auth;

/// <summary>Result of a successful register or login: the JWT and who it belongs to.</summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserResponseDto User { get; set; } = null!;
}

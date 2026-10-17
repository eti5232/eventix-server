namespace Eventix.Application.DTOs.Auth;

/// <summary>Public information about a user. Never includes the password hash.</summary>
public class UserResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

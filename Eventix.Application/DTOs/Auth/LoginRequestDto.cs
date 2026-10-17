using System.ComponentModel.DataAnnotations;

namespace Eventix.Application.DTOs.Auth;

/// <summary>Input for logging in. No strength rules here: a wrong password is simply rejected as invalid credentials.</summary>
public class LoginRequestDto
{
    [Required, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128)]
    public string Password { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace Eventix.Application.DTOs.Auth;

/// <summary>Input for registering a new account. There is no role field: every new account is a Producer.</summary>
public class RegisterRequestDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    /// <summary>8 to 72 characters (BCrypt ignores anything after 72), with at least one letter and one digit.</summary>
    [Required, StringLength(72, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "The password must contain at least one letter and one digit.")]
    public string Password { get; set; } = string.Empty;
}

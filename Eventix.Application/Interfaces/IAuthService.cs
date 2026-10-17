using Eventix.Application.DTOs.Auth;

namespace Eventix.Application.Interfaces;

/// <summary>Registration and login.</summary>
public interface IAuthService
{
    /// <summary>Creates a Producer account and returns a token for it.</summary>
    /// <exception cref="Exceptions.ConflictException">The email is already registered.</exception>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);

    /// <summary>Checks the credentials and returns a token.</summary>
    /// <exception cref="Exceptions.UnauthorizedException">Unknown email or wrong password (the same message for both).</exception>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);
}

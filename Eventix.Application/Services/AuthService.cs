using AutoMapper;
using Eventix.Application.DTOs.Auth;
using Eventix.Application.Exceptions;
using Eventix.Application.Interfaces;
using Eventix.Domain.Entities;
using Eventix.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Eventix.Application.Services;

/// <summary>Registration and login.</summary>
public class AuthService : IAuthService
{
    /// <summary>One message for both "unknown email" and "wrong password", so the answer does not reveal which accounts exist.</summary>
    private const string InvalidCredentialsMessage = "Invalid email or password.";

    /// <summary>Shared with the UnitOfWork, which reports the same error when two registrations race on one email.</summary>
    public const string EmailAlreadyRegisteredMessage = "An account with this email already exists.";

    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IMapper _mapper;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IMapper mapper,
        TimeProvider timeProvider,
        ILogger<AuthService> logger)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _mapper = mapper;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        if (await _users.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException(EmailAlreadyRegisteredMessage);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.Producer, // registration can never create an Admin
            CreatedAtUtc = _timeProvider.GetUtcNow().UtcDateTime
        };

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} registered.", user.Id);
        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(NormalizeEmail(request.Email), cancellationToken);

        if (user is null)
        {
            // Same amount of work as a real check, to hide whether the email exists.
            _passwordHasher.SimulateVerification(request.Password);
            _logger.LogWarning("Login failed.");
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for user {UserId}.", user.Id);
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        _logger.LogInformation("User {UserId} logged in.", user.Id);
        return BuildResponse(user);
    }

    /// <summary>Emails are stored and compared in lowercase, so "A@x.com" and "a@x.com" are the same account.</summary>
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private AuthResponseDto BuildResponse(User user)
    {
        var token = _tokenGenerator.Generate(user);
        return new AuthResponseDto
        {
            Token = token.Value,
            ExpiresAtUtc = token.ExpiresAtUtc,
            User = _mapper.Map<UserResponseDto>(user)
        };
    }
}

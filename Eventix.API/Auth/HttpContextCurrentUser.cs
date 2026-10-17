using Eventix.Application.Common;
using Eventix.Application.Exceptions;
using Eventix.Application.Interfaces;
using Eventix.Domain.Enums;

namespace Eventix.API.Auth;

/// <summary><see cref="ICurrentUser"/> that reads the claims of the JWT-authenticated user of the current HTTP request.</summary>
public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int Id
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirst(AuthClaims.UserId)?.Value;
            return int.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedException("Authentication is required.");
        }
    }

    public bool IsAdmin =>
        _httpContextAccessor.HttpContext?.User.FindFirst(AuthClaims.Role)?.Value == nameof(UserRole.Admin);
}

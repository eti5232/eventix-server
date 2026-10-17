using Eventix.Application.DTOs.Auth;
using Eventix.Domain.Entities;

namespace Eventix.Application.Interfaces;

/// <summary>Creates signed JWTs. The implementation lives in Infrastructure.</summary>
public interface IJwtTokenGenerator
{
    /// <summary>Creates a token for the user, with their id, email, name and role as claims.</summary>
    JwtToken Generate(User user);
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eventix.Application.Common;
using Eventix.Application.DTOs.Auth;
using Eventix.Application.Interfaces;
using Eventix.Application.Options;
using Eventix.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Eventix.Infrastructure.Auth;

/// <summary>Creates HS256-signed JWTs. Times come from <see cref="TimeProvider"/>, always in UTC.</summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;

    public JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public JwtToken Generate(User user)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var expiresAtUtc = now.AddMinutes(_options.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(AuthClaims.UserId, user.Id.ToString()),
            new Claim(AuthClaims.Email, user.Email),
            new Claim(AuthClaims.Name, user.FullName),
            new Claim(AuthClaims.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}

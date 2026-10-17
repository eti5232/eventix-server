using System.Text;
using Eventix.API.Errors;
using Eventix.Application.Common;
using Eventix.Application.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Eventix.API.Extensions;

/// <summary>Setup of JWT bearer authentication.</summary>
public static class AuthenticationExtensions
{
    private const int MinimumKeyBytes = 32;

    /// <summary>
    /// Makes the API validate JWTs on every request: signature, issuer, audience and expiry.
    /// Fails at startup (before any request) if the JWT settings are missing or the key is too weak.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        EnsureSettingsAreValid(jwt);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep the claim names exactly as our generator wrote them ("sub", "role"), without .NET renaming them.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = AuthClaims.Name,
                    RoleClaimType = AuthClaims.Role
                };

                // 401 and 403 produced by the authentication middleware use the same JSON error format as everything else.
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse(); // replaces the default empty 401
                        context.Response.Headers.WWWAuthenticate = JwtBearerDefaults.AuthenticationScheme;
                        var detail = context.AuthenticateFailure is SecurityTokenExpiredException
                            ? "The token has expired. Please log in again."
                            : "Authentication is required. Send a valid Bearer token.";
                        return ErrorResponseFactory.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, detail);
                    },
                    OnForbidden = context => ErrorResponseFactory.WriteAsync(
                        context.HttpContext, StatusCodes.Status403Forbidden, "You do not have permission to perform this action.")
                };
            });

        return services;
    }

    /// <summary>The messages name the setting but never print its value.</summary>
    private static void EnsureSettingsAreValid(JwtOptions jwt)
    {
        if (Encoding.UTF8.GetByteCount(jwt.Key) < MinimumKeyBytes)
            throw new InvalidOperationException(
                $"The setting 'Jwt:Key' is missing or shorter than {MinimumKeyBytes} bytes. Set it with user-secrets or an environment variable.");

        if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience))
            throw new InvalidOperationException("The settings 'Jwt:Issuer' and 'Jwt:Audience' must be set.");
    }
}

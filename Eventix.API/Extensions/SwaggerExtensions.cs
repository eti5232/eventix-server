using Microsoft.OpenApi.Models;

namespace Eventix.API.Extensions;

/// <summary>Swagger setup.</summary>
public static class SwaggerExtensions
{
    /// <summary>Adds Swagger with an "Authorize" button, so protected endpoints can be tried with a JWT.</summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Paste the token only. Swagger adds the 'Bearer' prefix.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };

            options.AddSecurityDefinition("Bearer", bearerScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { { bearerScheme, Array.Empty<string>() } });
        });

        return services;
    }
}

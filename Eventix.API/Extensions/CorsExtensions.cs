namespace Eventix.API.Extensions;

/// <summary>Setup of CORS, so the browser lets the React client (another origin) call this API.</summary>
public static class CorsExtensions
{
    /// <summary>The config section holding the list of allowed client origins.</summary>
    public const string AllowedOriginsKey = "Cors:AllowedOrigins";

    private const string PolicyName = "EventixClient";

    /// <summary>
    /// Allows only the origins listed in <c>Cors:AllowedOrigins</c> (never "any origin").
    /// Fails at startup if the list is empty, instead of silently blocking every browser request.
    /// </summary>
    public static IServiceCollection AddClientCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = (configuration.GetSection(AllowedOriginsKey).Get<string[]>() ?? Array.Empty<string>())
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            // A trailing slash would never match the browser's Origin header.
            .Select(origin => origin.Trim().TrimEnd('/'))
            .ToArray();

        if (origins.Length == 0)
        {
            throw new InvalidOperationException(
                $"No client origins are configured. Set '{AllowedOriginsKey}' (for example http://localhost:5173).");
        }

        services.AddCors(options => options.AddPolicy(PolicyName, policy => policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            // Lets the client read the correlation id header from error responses.
            .WithExposedHeaders("X-Correlation-Id")));

        return services;
    }

    /// <summary>Applies the CORS policy. Must run after routing/Swagger and before authentication.</summary>
    public static IApplicationBuilder UseClientCors(this IApplicationBuilder app) => app.UseCors(PolicyName);
}

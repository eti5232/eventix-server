namespace Eventix.API.Middleware;

/// <summary>
/// Shared pieces of the correlation id: its names, how to read it from the request,
/// and how to attach it to log lines. Written once so every middleware uses the same logic.
/// </summary>
public static class CorrelationIdExtensions
{
    /// <summary>Name of the HTTP header that carries the correlation id.</summary>
    public const string HeaderName = "X-Correlation-Id";

    /// <summary>Key in <see cref="HttpContext.Items"/> and name of the logging-scope property (used by <c>${scopeproperty:CorrelationId}</c>).</summary>
    public const string ItemKey = "CorrelationId";

    /// <summary>The correlation id of the current request (falls back to the trace identifier if the middleware did not run).</summary>
    public static string GetCorrelationId(this HttpContext context) =>
        context.Items[ItemKey] as string ?? context.TraceIdentifier;

    /// <summary>Opens a logging scope, so every log line written inside it carries the correlation id.</summary>
    public static IDisposable? BeginCorrelationScope(this ILogger logger, string correlationId) =>
        logger.BeginScope(new Dictionary<string, object> { [ItemKey] = correlationId });
}

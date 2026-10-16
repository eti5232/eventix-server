namespace Eventix.API.Middleware;

/// <summary>
/// Gives every request a correlation id: it keeps the client's <c>X-Correlation-Id</c> if it is safe,
/// otherwise creates a new one. The id is returned in the response header and attached to all log lines of the request.
/// </summary>
public class CorrelationIdMiddleware
{
    private const int MaxLength = 64;

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ReadFromHeaderOrCreate(context);

        context.Items[CorrelationIdExtensions.ItemKey] = correlationId;
        context.Response.Headers[CorrelationIdExtensions.HeaderName] = correlationId;

        using (_logger.BeginCorrelationScope(correlationId))
        {
            await _next(context);
        }
    }

    /// <summary>
    /// The header value goes into the log files, so a client-supplied value is accepted only if it is short
    /// and made of letters, digits, '-' and '_'. Anything else (spaces, line breaks, symbols) is replaced.
    /// </summary>
    private static string ReadFromHeaderOrCreate(HttpContext context)
    {
        var supplied = context.Request.Headers[CorrelationIdExtensions.HeaderName].ToString();
        return IsSafe(supplied) ? supplied : Guid.NewGuid().ToString("N");
    }

    private static bool IsSafe(string value) =>
        value.Length is > 0 and <= MaxLength && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');
}

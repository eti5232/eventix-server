using System.Diagnostics;

namespace Eventix.API.Middleware;

/// <summary>
/// Writes one Information log line per request: method, path, final status code and duration.
/// It never logs the query string, headers or body, because they may contain passwords or tokens.
/// </summary>
public class RequestLoggingMiddleware
{
    private const int MaxLoggedPathLength = 200;

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Logging when the response is completed (and not in a finally block) gives the real final status code,
        // including the one set later by the exception-handling middleware.
        context.Response.OnCompleted(() =>
        {
            stopwatch.Stop();

            // The correlation scope has already ended by now, so it is opened again for this log line.
            using (_logger.BeginCorrelationScope(context.GetCorrelationId()))
            {
                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                    context.Request.Method,
                    GetSafePath(context),
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }

            return Task.CompletedTask;
        });

        return _next(context);
    }

    /// <summary>Path only (no query string). It is re-encoded, so a line break in the URL cannot forge a log line.</summary>
    private static string GetSafePath(HttpContext context)
    {
        var path = context.Request.Path.ToUriComponent();
        return path.Length <= MaxLoggedPathLength ? path : path[..MaxLoggedPathLength] + "...";
    }
}

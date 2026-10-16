using Eventix.API.Errors;
using Eventix.Application.Exceptions;

namespace Eventix.API.Middleware;

/// <summary>
/// Catches every exception thrown later in the pipeline and turns it into a uniform JSON error response
/// with the right status code. It must be the first middleware, so nothing escapes it.
/// Unexpected exceptions become a generic 500: the details go to the log, never to the client.
/// </summary>
public class ExceptionHandlingMiddleware
{
    /// <summary>Non-standard status (used by nginx) for "the client closed the connection"; nobody reads it, it only shows in the log.</summary>
    private const int ClientClosedRequest = 499;

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client went away; there is nobody to answer and it is not an error.
            using (_logger.BeginCorrelationScope(context.GetCorrelationId()))
            {
                _logger.LogInformation("The request was cancelled by the client.");
            }

            context.Response.StatusCode = ClientClosedRequest;
        }
        catch (Exception exception) when (context.Response.HasStarted)
        {
            // Part of the response was already sent, so a new status or body is impossible.
            // Log it and let the server abort the connection.
            using (_logger.BeginCorrelationScope(context.GetCorrelationId()))
            {
                _logger.LogError(exception, "Exception after the response had started; no error response could be written.");
            }

            throw;
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var statusCode = MapToStatusCode(exception);

        // The correlation scope of the inner middleware has already ended, so it is opened again here.
        using (_logger.BeginCorrelationScope(context.GetCorrelationId()))
        {
            Log(exception, statusCode);
        }

        var detail = statusCode >= StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred. Please report the correlation id."
            : exception.Message;

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(
            ErrorResponseFactory.Create(context, statusCode, detail),
            options: (System.Text.Json.JsonSerializerOptions?)null,
            contentType: ErrorResponseFactory.ContentType);
    }

    /// <summary>The one place that decides which exception means which HTTP status.</summary>
    private static int MapToStatusCode(Exception exception) => exception switch
    {
        NotFoundException => StatusCodes.Status404NotFound,
        ConflictException => StatusCodes.Status409Conflict,
        BusinessValidationException => StatusCodes.Status400BadRequest,
        ForbiddenException => StatusCodes.Status403Forbidden,
        UnauthorizedException => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status500InternalServerError
    };

    /// <summary>Error for unexpected failures (with the full exception), Warning for conflicts, Information for the other expected rejections.</summary>
    private void Log(Exception exception, int statusCode)
    {
        if (statusCode >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception while processing the request.");
        else if (statusCode == StatusCodes.Status409Conflict)
            _logger.LogWarning("Request rejected with {StatusCode}: {Message}", statusCode, exception.Message);
        else
            _logger.LogInformation("Request rejected with {StatusCode}: {Message}", statusCode, exception.Message);
    }
}

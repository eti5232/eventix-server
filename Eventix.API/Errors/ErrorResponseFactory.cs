using Eventix.API.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;

namespace Eventix.API.Errors;

/// <summary>
/// The single place that defines how every error response looks: a ProblemDetails JSON
/// with the status, a title, a detail message and the correlation id.
/// Used by the exception middleware and by the model-validation (400) response, so both have the same shape.
/// </summary>
public static class ErrorResponseFactory
{
    /// <summary>Content type of every error response.</summary>
    public const string ContentType = "application/problem+json";

    private const string CorrelationIdProperty = "correlationId";

    /// <summary>Creates an error response for the given status code. The title is the standard reason phrase.</summary>
    public static ProblemDetails Create(HttpContext context, int statusCode, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = detail
        };
        problem.Extensions[CorrelationIdProperty] = context.GetCorrelationId();
        return problem;
    }

    /// <summary>Sets the status code and writes the uniform error body to the response.</summary>
    public static Task WriteAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(
            Create(context, statusCode, detail),
            options: (System.Text.Json.JsonSerializerOptions?)null,
            contentType: ContentType);
    }

    /// <summary>Creates a 400 response that also lists the invalid fields in <c>errors</c>.</summary>
    public static ValidationProblemDetails CreateValidation(HttpContext context, ModelStateDictionary modelState)
    {
        var problem = new ValidationProblemDetails(modelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status400BadRequest),
            Detail = "One or more validation errors occurred."
        };
        problem.Extensions[CorrelationIdProperty] = context.GetCorrelationId();
        return problem;
    }
}

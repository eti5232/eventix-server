using Eventix.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Eventix.API.Filters;

/// <summary>
/// TEMPORARY (stage 3 only): turns NotFoundException into 404 and ConflictException into 409,
/// so these cases are not reported as 500. Stage 4 replaces it with ExceptionHandlingMiddleware and deletes this file.
/// </summary>
public class TemporaryExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var (status, title) = context.Exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (0, string.Empty)
        };

        if (status == 0)
            return;

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = context.Exception.Message
        })
        { StatusCode = status };
        context.ExceptionHandled = true;
    }
}

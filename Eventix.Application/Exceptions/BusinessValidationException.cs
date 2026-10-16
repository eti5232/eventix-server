namespace Eventix.Application.Exceptions;

/// <summary>Thrown when a request is well-formed but breaks a business rule (e.g. a date in the past). Translated to HTTP 400.</summary>
public class BusinessValidationException : Exception
{
    public BusinessValidationException(string message) : base(message)
    {
    }
}

namespace Eventix.Application.Exceptions;

/// <summary>Thrown when the caller could not be authenticated (e.g. wrong credentials). Translated to HTTP 401.</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}

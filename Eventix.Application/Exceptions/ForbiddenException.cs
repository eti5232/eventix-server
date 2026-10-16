namespace Eventix.Application.Exceptions;

/// <summary>Thrown when the user is authenticated but not allowed to do this (e.g. not the owner). Translated to HTTP 403.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}

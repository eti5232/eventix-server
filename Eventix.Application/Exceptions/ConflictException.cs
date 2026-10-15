namespace Eventix.Application.Exceptions;

/// <summary>Thrown when a request conflicts with the current state of the data. Translated to HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

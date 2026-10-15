namespace Eventix.Application.Exceptions;

/// <summary>Thrown when a requested resource does not exist. Translated to HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    /// <summary>Creates the exception with a uniform message, e.g. "Equipment with id 7 was not found."</summary>
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with id {key} was not found.")
    {
    }
}

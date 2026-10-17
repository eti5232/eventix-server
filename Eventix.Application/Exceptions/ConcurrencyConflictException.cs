namespace Eventix.Application.Exceptions;

/// <summary>
/// Thrown when someone else changed the same data at the same moment (a lost optimistic-concurrency race).
/// Translated to HTTP 409; the user can simply try again.
/// </summary>
public class ConcurrencyConflictException : ConflictException
{
    public ConcurrencyConflictException()
        : base("Someone else changed the same data at the same moment. Please try again.")
    {
    }
}

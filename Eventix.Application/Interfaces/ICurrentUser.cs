namespace Eventix.Application.Interfaces;

/// <summary>The user making the current request, as taken from the validated JWT. Lets services check ownership and roles.</summary>
public interface ICurrentUser
{
    /// <summary>The id of the user.</summary>
    /// <exception cref="Exceptions.UnauthorizedException">There is no authenticated user.</exception>
    int Id { get; }

    /// <summary>True when the user has the Admin role.</summary>
    bool IsAdmin { get; }
}

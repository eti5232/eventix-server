namespace Eventix.Application.Interfaces;

/// <summary>Password hashing. The implementation (BCrypt) lives in Infrastructure.</summary>
public interface IPasswordHasher
{
    /// <summary>Creates a salted hash of the password.</summary>
    string Hash(string password);

    /// <summary>Checks a password against a stored hash.</summary>
    bool Verify(string password, string passwordHash);

    /// <summary>
    /// Spends the same time as <see cref="Verify"/> without any real hash.
    /// Called when the email is unknown, so response time does not reveal whether an account exists.
    /// </summary>
    void SimulateVerification(string password);
}

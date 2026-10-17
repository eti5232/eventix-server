using Eventix.Application.Interfaces;

namespace Eventix.Infrastructure.Auth;

/// <summary>BCrypt implementation of <see cref="IPasswordHasher"/>. BCrypt adds a random salt to every hash.</summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    /// <summary>Cost of the hash: each +1 doubles the time. Real hashes and the dummy one must use the same value.</summary>
    private const int WorkFactor = 11;

    /// <summary>A valid hash of a random password, created once. Used only to burn the same time as a real check.</summary>
    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"), WorkFactor);

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);

    public void SimulateVerification(string password) => BCrypt.Net.BCrypt.Verify(password, DummyHash);
}

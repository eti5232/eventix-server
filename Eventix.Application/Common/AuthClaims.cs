namespace Eventix.Application.Common;

/// <summary>
/// Names of the claims inside our JWT. Written once, so the token generator (which writes them)
/// and the API setup and services (which read them) can never disagree.
/// </summary>
public static class AuthClaims
{
    public const string UserId = "sub";
    public const string Email = "email";
    public const string Name = "name";
    public const string Role = "role";
}

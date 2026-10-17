using Eventix.Domain.Entities;

namespace Eventix.Application.Interfaces;

/// <summary>Data access for users. Emails passed in are already normalized (lowercase). Saving is done through <see cref="IUnitOfWork"/>.</summary>
public interface IUserRepository
{
    /// <summary>The user with this email, read-only, or null if there is none.</summary>
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);
}

using Eventix.Domain.Entities;

namespace Eventix.Application.Interfaces;

/// <summary>Data access for categories. Saving is done through <see cref="IUnitOfWork"/>.</summary>
public interface ICategoryRepository
{
    /// <summary>All categories ordered by name, read-only.</summary>
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>A category for reading only, or null if it does not exist.</summary>
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>A tracked category that can be modified, or null if it does not exist.</summary>
    Task<Category?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    /// <summary>Whether a category with this name exists, optionally ignoring one category (for updates).</summary>
    Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken);

    Task AddAsync(Category category, CancellationToken cancellationToken);
}

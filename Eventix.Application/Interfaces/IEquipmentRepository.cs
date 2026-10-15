using Eventix.Application.DTOs.Equipment;
using Eventix.Domain.Entities;

namespace Eventix.Application.Interfaces;

/// <summary>Data access for equipment. Saving is done through <see cref="IUnitOfWork"/>.</summary>
public interface IEquipmentRepository
{
    /// <summary>
    /// One filtered and sorted page of active equipment (with its category), read-only,
    /// plus the total number of matching items. Paging is done inside the database query.
    /// </summary>
    Task<(IReadOnlyList<Equipment> Items, int TotalCount)> GetPagedAsync(
        EquipmentQuery query, CancellationToken cancellationToken);

    /// <summary>An equipment item with its category for reading only, or null if it does not exist.</summary>
    Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>A tracked equipment item with its category that can be modified, or null if it does not exist.</summary>
    Task<Equipment?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(Equipment equipment, CancellationToken cancellationToken);
}

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

    /// <summary>The active (not removed) equipment items with these ids, read-only, in one query. Missing or removed ids are simply not returned.</summary>
    Task<IReadOnlyList<Equipment>> GetActiveByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    /// <summary>
    /// Takes a shared lock on these equipment rows until the current transaction ends. Many reservations can hold it together,
    /// but nobody can change or remove the equipment meanwhile. Must be called inside <see cref="IUnitOfWork.ExecuteInTransactionAsync{T}"/>.
    /// </summary>
    Task LockForShareAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    /// <summary>
    /// Takes an exclusive lock on this equipment row until the current transaction ends: it waits for reservations being created
    /// and blocks new ones, so the checks made afterwards cannot go stale. Must be called inside <see cref="IUnitOfWork.ExecuteInTransactionAsync{T}"/>.
    /// </summary>
    Task LockForUpdateAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(Equipment equipment, CancellationToken cancellationToken);
}

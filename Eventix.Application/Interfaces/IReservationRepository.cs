using Eventix.Application.DTOs.Reservations;
using Eventix.Domain.Entities;

namespace Eventix.Application.Interfaces;

/// <summary>Data access for reservations. Saving is done through <see cref="IUnitOfWork"/>.</summary>
public interface IReservationRepository
{
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken);

    /// <summary>A reservation with its items, their equipment and its user, read-only, or null if it does not exist.</summary>
    Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>A tracked reservation with its items and their equipment, which can be modified, or null if it does not exist.</summary>
    Task<Reservation?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken);

    /// <summary>How many Confirmed reservations that contain this equipment have not ended yet (their end date is today or later).</summary>
    Task<int> CountActiveForEquipmentAsync(int equipmentId, DateOnly today, CancellationToken cancellationToken);

    /// <summary>
    /// One page of reservations, newest first, read-only, with items and equipment loaded in the same query, plus the total count.
    /// Pass a user id to get only that user's reservations, or null for everyone's.
    /// </summary>
    Task<(IReadOnlyList<Reservation> Items, int TotalCount)> GetPagedAsync(
        int? userId, ReservationQuery query, CancellationToken cancellationToken);
}

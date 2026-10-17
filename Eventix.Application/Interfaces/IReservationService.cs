using Eventix.Application.Common;
using Eventix.Application.DTOs.Reservations;
using Eventix.Domain.Enums;

namespace Eventix.Application.Interfaces;

/// <summary>Business logic for reservations.</summary>
public interface IReservationService
{
    /// <summary>Reserves all the requested items for the whole date range, or nothing at all.</summary>
    /// <exception cref="Exceptions.BusinessValidationException">Bad dates (past, reversed, longer than 7 days), no items, or duplicate items.</exception>
    /// <exception cref="Exceptions.NotFoundException">An equipment item does not exist or was removed.</exception>
    /// <exception cref="Exceptions.InsufficientStockException">Some item has too few free units on some day.</exception>
    /// <exception cref="Exceptions.ConcurrencyConflictException">Someone else reserved the same stock at the same moment.</exception>
    Task<ReservationResponseDto> CreateAsync(CreateReservationRequestDto request, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels (owner or admin, before the start date, gives the stock back) or marks as returned (admin only, once the reservation
    /// has started). The return day is recorded; an early return frees the stock from the following day. Cancelled and Returned are final.
    /// </summary>
    /// <exception cref="Exceptions.BusinessValidationException">The requested status is not Cancelled or Returned.</exception>
    /// <exception cref="Exceptions.NotFoundException">The reservation does not exist.</exception>
    /// <exception cref="Exceptions.ForbiddenException">The caller is not allowed to make this change.</exception>
    /// <exception cref="Exceptions.ConflictException">The change is not allowed in the reservation's current state or date.</exception>
    /// <exception cref="Exceptions.ConcurrencyConflictException">Someone else changed the same reservation or stock at the same moment.</exception>
    Task<ReservationResponseDto> ChangeStatusAsync(int id, ReservationStatus newStatus, CancellationToken cancellationToken);

    /// <summary>A producer gets only their own reservations; an admin gets everyone's.</summary>
    Task<PagedResult<ReservationResponseDto>> GetPagedAsync(ReservationQuery query, CancellationToken cancellationToken);

    /// <exception cref="Exceptions.NotFoundException">The reservation does not exist.</exception>
    /// <exception cref="Exceptions.ForbiddenException">The reservation belongs to another user and the caller is not an admin.</exception>
    Task<ReservationResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken);
}

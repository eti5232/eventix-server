using Eventix.Domain.Entities;

namespace Eventix.Application.Interfaces;

/// <summary>An equipment item and how many units of it to reserve or release.</summary>
public record StockRequest(Equipment Equipment, int Quantity);

/// <summary>
/// The single place that reserves and releases stock over a date range. Creating, cancelling and replacing items
/// in a reservation all use it. It only changes tracked entities; saving is the caller's job (one call to the unit of work).
/// </summary>
public interface IStockAllocator
{
    /// <summary>
    /// Reserves the quantities on every day of the range, for all items, or for none: if any item on any day has
    /// too little free stock, nothing is changed.
    /// </summary>
    /// <exception cref="Exceptions.InsufficientStockException">Some item has too few free units on some day.</exception>
    Task ReserveAsync(IReadOnlyCollection<StockRequest> items, DateOnly start, DateOnly end, CancellationToken cancellationToken);

    /// <summary>Gives the quantities back on every day of the range, for all items, or for none.</summary>
    /// <exception cref="InvalidOperationException">A stock row is missing (inconsistent data).</exception>
    /// <exception cref="Exceptions.ConcurrencyConflictException">A stock row holds less than is being released: someone else already released it.</exception>
    Task ReleaseAsync(IReadOnlyCollection<StockRequest> items, DateOnly start, DateOnly end, CancellationToken cancellationToken);
}

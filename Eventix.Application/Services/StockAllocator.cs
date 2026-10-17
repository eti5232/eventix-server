using Eventix.Application.Common;
using Eventix.Application.Exceptions;
using Eventix.Application.Interfaces;
using Eventix.Domain.Entities;

namespace Eventix.Application.Services;

/// <summary>Reserves and releases stock on the daily stock rows. See <see cref="IStockAllocator"/>.</summary>
public class StockAllocator : IStockAllocator
{
    private readonly IDailyStockRepository _stock;

    public StockAllocator(IDailyStockRepository stock)
    {
        _stock = stock;
    }

    public async Task ReserveAsync(
        IReadOnlyCollection<StockRequest> items, DateOnly start, DateOnly end, CancellationToken cancellationToken)
    {
        var rows = await LoadRowsAsync(items, start, end, createMissing: true, cancellationToken);

        // First check everything, then change anything, so a failure leaves no half-done reservation behind.
        foreach (var item in items)
        {
            foreach (var day in DateRangeRules.Days(start, end))
            {
                var row = rows[(item.Equipment.Id, day)];
                var available = item.Equipment.TotalQuantity - row.ReservedQuantity;
                if (item.Quantity > available)
                    throw new InsufficientStockException(item.Equipment.Name, day, item.Quantity, available);
            }
        }

        foreach (var item in items)
            foreach (var day in DateRangeRules.Days(start, end))
                rows[(item.Equipment.Id, day)].ReservedQuantity += item.Quantity;
    }

    public async Task ReleaseAsync(
        IReadOnlyCollection<StockRequest> items, DateOnly start, DateOnly end, CancellationToken cancellationToken)
    {
        var rows = await LoadRowsAsync(items, start, end, createMissing: false, cancellationToken);

        foreach (var item in items)
        {
            foreach (var day in DateRangeRules.Days(start, end))
            {
                // A reservation that exists must have its stock rows. Rows are never deleted, so a missing one is a real inconsistency.
                if (!rows.TryGetValue((item.Equipment.Id, day), out var row))
                    throw new InvalidOperationException(
                        $"Cannot release {item.Quantity} of equipment {item.Equipment.Id} on {day:yyyy-MM-dd}: the stock record is missing.");

                // The reservation was read a moment before the stock rows. If the stock now holds less than this reservation
                // took, another request has just released it (for example a parallel cancel that already committed).
                // That is a lost race, not corruption: the caller gets a 409 and nothing is saved.
                if (row.ReservedQuantity < item.Quantity)
                    throw new ConcurrencyConflictException();
            }
        }

        foreach (var item in items)
            foreach (var day in DateRangeRules.Days(start, end))
                rows[(item.Equipment.Id, day)].ReservedQuantity -= item.Quantity;
    }

    private async Task<Dictionary<(int EquipmentId, DateOnly Date), DailyStock>> LoadRowsAsync(
        IReadOnlyCollection<StockRequest> items, DateOnly start, DateOnly end, bool createMissing, CancellationToken cancellationToken)
    {
        var equipmentIds = items.Select(i => i.Equipment.Id).Distinct().ToList();

        var rows = createMissing
            ? await _stock.GetOrCreateForRangeAsync(equipmentIds, start, end, cancellationToken)
            : await _stock.GetForRangeAsync(equipmentIds, start, end, cancellationToken);

        return rows.ToDictionary(row => (row.EquipmentId, row.Date));
    }
}

using Eventix.Application.Common;
using Eventix.Application.Interfaces;
using Eventix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eventix.Infrastructure.Repositories;

/// <summary>EF Core implementation of <see cref="IDailyStockRepository"/>.</summary>
public class DailyStockRepository : IDailyStockRepository
{
    private readonly AppDbContext _context;

    public DailyStockRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DailyStock>> GetForRangeAsync(
        IReadOnlyCollection<int> equipmentIds, DateOnly start, DateOnly end, CancellationToken cancellationToken) =>
        await _context.DailyStocks
            .Where(s => equipmentIds.Contains(s.EquipmentId) && s.Date >= start && s.Date <= end)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DailyStock>> GetOrCreateForRangeAsync(
        IReadOnlyCollection<int> equipmentIds, DateOnly start, DateOnly end, CancellationToken cancellationToken)
    {
        var rows = (await GetForRangeAsync(equipmentIds, start, end, cancellationToken)).ToList();
        var existing = rows.Select(row => (row.EquipmentId, row.Date)).ToHashSet();

        var missing = (
            from equipmentId in equipmentIds.Distinct()
            from day in DateRangeRules.Days(start, end)
            where !existing.Contains((equipmentId, day))
            select new DailyStock { EquipmentId = equipmentId, Date = day, ReservedQuantity = 0 }).ToList();

        _context.DailyStocks.AddRange(missing);
        rows.AddRange(missing);
        return rows;
    }

    public async Task<int> GetMaxReservedQuantityFromAsync(int equipmentId, DateOnly fromDate, CancellationToken cancellationToken) =>
        await _context.DailyStocks
            .AsNoTracking()
            .Where(s => s.EquipmentId == equipmentId && s.Date >= fromDate)
            .MaxAsync(s => (int?)s.ReservedQuantity, cancellationToken) ?? 0;

    public async Task<int> GetMaxReservedQuantityAsync(
        int equipmentId, DateOnly start, DateOnly end, CancellationToken cancellationToken) =>
        await _context.DailyStocks
            .AsNoTracking()
            .Where(s => s.EquipmentId == equipmentId && s.Date >= start && s.Date <= end)
            .MaxAsync(s => (int?)s.ReservedQuantity, cancellationToken) ?? 0;
}

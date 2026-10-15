using Eventix.Application.Common;
using Eventix.Application.DTOs.Equipment;
using Eventix.Application.Interfaces;
using Eventix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eventix.Infrastructure.Repositories;

/// <summary>EF Core implementation of <see cref="IEquipmentRepository"/>.</summary>
public class EquipmentRepository : IEquipmentRepository
{
    private readonly AppDbContext _context;

    public EquipmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Equipment> Items, int TotalCount)> GetPagedAsync(
        EquipmentQuery query, CancellationToken cancellationToken)
    {
        var equipment = _context.Equipment.AsNoTracking().Where(e => e.IsActive);

        if (query.CategoryId.HasValue)
            equipment = equipment.Where(e => e.CategoryId == query.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            equipment = equipment.Where(e =>
                e.Name.ToLower().Contains(search) ||
                (e.Description != null && e.Description.ToLower().Contains(search)));
        }

        if (query.MinPrice.HasValue)
            equipment = equipment.Where(e => e.DailyPrice >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            equipment = equipment.Where(e => e.DailyPrice <= query.MaxPrice.Value);

        var totalCount = await equipment.CountAsync(cancellationToken);

        var descending = query.SortDir == SortDirection.Desc;
        var sorted = query.SortBy switch
        {
            EquipmentSortBy.Price => descending
                ? equipment.OrderByDescending(e => e.DailyPrice)
                : equipment.OrderBy(e => e.DailyPrice),
            _ => descending
                ? equipment.OrderByDescending(e => e.Name)
                : equipment.OrderBy(e => e.Name)
        };

        // Id as a tie-breaker keeps the order stable between pages.
        var items = await sorted
            .ThenBy(e => e.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(e => e.Category)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        _context.Equipment.AsNoTracking().Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<Equipment?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken) =>
        _context.Equipment.Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task AddAsync(Equipment equipment, CancellationToken cancellationToken) =>
        await _context.Equipment.AddAsync(equipment, cancellationToken);
}

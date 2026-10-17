using Eventix.Application.DTOs.Reservations;
using Eventix.Application.Interfaces;
using Eventix.Domain.Entities;
using Eventix.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Eventix.Infrastructure.Repositories;

/// <summary>EF Core implementation of <see cref="IReservationRepository"/>.</summary>
public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken) =>
        await _context.Reservations.AddAsync(reservation, cancellationToken);

    public Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        _context.Reservations.AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Items).ThenInclude(i => i.Equipment)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<Reservation?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken) =>
        _context.Reservations
            .Include(r => r.Items).ThenInclude(i => i.Equipment)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<int> CountActiveForEquipmentAsync(int equipmentId, DateOnly today, CancellationToken cancellationToken) =>
        _context.Reservations.CountAsync(
            r => r.Status == ReservationStatus.Confirmed
                 && r.EndDate >= today
                 && r.Items.Any(i => i.EquipmentId == equipmentId),
            cancellationToken);

    public async Task<(IReadOnlyList<Reservation> Items, int TotalCount)> GetPagedAsync(
        int? userId, ReservationQuery query, CancellationToken cancellationToken)
    {
        var reservations = _context.Reservations.AsNoTracking().AsQueryable();

        if (userId.HasValue)
            reservations = reservations.Where(r => r.UserId == userId.Value);

        if (query.Status.HasValue)
            reservations = reservations.Where(r => r.Status == query.Status.Value);

        var totalCount = await reservations.CountAsync(cancellationToken);

        // Newest first; Id as a tie-breaker keeps the order stable between pages.
        var items = await reservations
            .OrderByDescending(r => r.CreatedAtUtc).ThenByDescending(r => r.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(r => r.User)
            .Include(r => r.Items).ThenInclude(i => i.Equipment)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

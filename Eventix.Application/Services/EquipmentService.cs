using AutoMapper;
using Eventix.Application.Common;
using Eventix.Application.DTOs.Equipment;
using Eventix.Application.Exceptions;
using Eventix.Application.Interfaces;
using Eventix.Domain.Entities;

namespace Eventix.Application.Services;

/// <summary>Business logic for the equipment catalog.</summary>
public class EquipmentService : IEquipmentService
{
    private readonly IEquipmentRepository _equipment;
    private readonly ICategoryRepository _categories;
    private readonly IReservationRepository _reservations;
    private readonly IDailyStockRepository _dailyStock;
    private readonly IBusinessClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EquipmentService(
        IEquipmentRepository equipment,
        ICategoryRepository categories,
        IReservationRepository reservations,
        IDailyStockRepository dailyStock,
        IBusinessClock clock,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _equipment = equipment;
        _categories = categories;
        _reservations = reservations;
        _dailyStock = dailyStock;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AvailabilityResponseDto> GetAvailabilityAsync(int id, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        DateRangeRules.EnsureValid(from, to);

        var equipment = await _equipment.GetByIdAsync(id, cancellationToken);
        if (equipment is null || !equipment.IsActive)
            throw new NotFoundException("Equipment", id);

        // A day without a stock row has nothing reserved, so the highest reserved quantity over the range is the one that limits us.
        var maxReserved = await _dailyStock.GetMaxReservedQuantityAsync(id, from, to, cancellationToken);

        return new AvailabilityResponseDto
        {
            EquipmentId = id,
            EquipmentName = equipment.Name,
            From = from,
            To = to,
            TotalQuantity = equipment.TotalQuantity,
            AvailableQuantity = Math.Max(0, equipment.TotalQuantity - maxReserved)
        };
    }

    public async Task<PagedResult<EquipmentResponseDto>> GetPagedAsync(
        EquipmentQuery query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _equipment.GetPagedAsync(query, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<EquipmentResponseDto>>(items);
        return new PagedResult<EquipmentResponseDto>(dtos, query.Page, query.PageSize, totalCount);
    }

    public async Task<EquipmentResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var equipment = await _equipment.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Equipment", id);
        return _mapper.Map<EquipmentResponseDto>(equipment);
    }

    public async Task<EquipmentResponseDto> CreateAsync(EquipmentRequestDto request, CancellationToken cancellationToken)
    {
        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        var equipment = _mapper.Map<Equipment>(request);
        await _equipment.AddAsync(equipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(equipment.Id, cancellationToken);
    }

    public async Task<EquipmentResponseDto> UpdateAsync(int id, EquipmentRequestDto request, CancellationToken cancellationToken)
    {
        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // The exclusive lock waits for reservations that are being created and blocks new ones until we commit,
            // so the check below sees every committed reservation and cannot go stale before the save.
            await _equipment.LockForUpdateAsync(id, cancellationToken);
            var equipment = await _equipment.GetByIdForUpdateAsync(id, cancellationToken)
                ?? throw new NotFoundException("Equipment", id);

            // The total can never drop below what is already reserved on a future day, or a day would hold more than exists.
            var reservedFromToday = await _dailyStock.GetMaxReservedQuantityFromAsync(id, _clock.Today, cancellationToken);
            if (request.TotalQuantity < reservedFromToday)
                throw new ConflictException(
                    $"The total quantity cannot be lower than {reservedFromToday}, the highest quantity already reserved on a day from today on.");

            _mapper.Map(request, equipment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        // Re-read so the response always shows the saved state, including the (possibly new) category name.
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Same protection as in UpdateAsync: lock first, then check, then save, all in one transaction.
            await _equipment.LockForUpdateAsync(id, cancellationToken);
            var equipment = await _equipment.GetByIdForUpdateAsync(id, cancellationToken)
                ?? throw new NotFoundException("Equipment", id);

            // An item that is still booked cannot disappear from under the people who reserved it.
            var activeReservations = await _reservations.CountActiveForEquipmentAsync(id, _clock.Today, cancellationToken);
            if (activeReservations > 0)
                throw new ConflictException(
                    $"'{equipment.Name}' cannot be removed: {activeReservations} confirmed reservation(s) containing it have not ended yet.");

            // Soft delete: the row stays for past reservations.
            equipment.IsActive = false;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }

    private async Task EnsureCategoryExistsAsync(int categoryId, CancellationToken cancellationToken)
    {
        if (!await _categories.ExistsAsync(categoryId, cancellationToken))
            throw new NotFoundException("Category", categoryId);
    }
}

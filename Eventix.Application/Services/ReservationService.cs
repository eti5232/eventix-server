using AutoMapper;
using Eventix.Application.Common;
using Eventix.Application.DTOs.Reservations;
using Eventix.Application.Exceptions;
using Eventix.Application.Interfaces;
using Eventix.Domain.Entities;
using Eventix.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Eventix.Application.Services;

/// <summary>Business logic for reservations: creating them (the contested part), listing and reading them.</summary>
public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservations;
    private readonly IEquipmentRepository _equipment;
    private readonly IStockAllocator _stockAllocator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessClock _clock;
    private readonly TimeProvider _timeProvider;
    private readonly IMapper _mapper;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(
        IReservationRepository reservations,
        IEquipmentRepository equipment,
        IStockAllocator stockAllocator,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IBusinessClock clock,
        TimeProvider timeProvider,
        IMapper mapper,
        ILogger<ReservationService> logger)
    {
        _reservations = reservations;
        _equipment = equipment;
        _stockAllocator = stockAllocator;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _clock = clock;
        _timeProvider = timeProvider;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReservationResponseDto> CreateAsync(CreateReservationRequestDto request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        var equipmentIds = request.Items.Select(i => i.EquipmentId).ToList();

        Reservation reservation;
        try
        {
            // The whole check-and-reserve is one transaction. Inside it the equipment rows are share-locked, so nobody can lower
            // an item's quantity or remove it between our reading it and saving. Reservations do not block each other.
            reservation = await _unitOfWork.ExecuteInTransactionAsync(
                () => ReserveAndSaveAsync(request, equipmentIds, cancellationToken), cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            _logger.LogWarning(
                "Reservation conflict: user {UserId} lost a race for equipment {EquipmentIds} from {StartDate} to {EndDate}.",
                _currentUser.Id, string.Join(",", equipmentIds), request.StartDate, request.EndDate);
            throw;
        }

        _logger.LogInformation(
            "Reservation {ReservationId} created by user {UserId} for {StartDate} to {EndDate}.",
            reservation.Id, reservation.UserId, request.StartDate, request.EndDate);

        // Read it back, so the response includes the equipment names.
        return await GetByIdAsync(reservation.Id, cancellationToken);
    }

    /// <summary>The part of creating a reservation that must run inside the transaction: lock, check, reserve and save.</summary>
    private async Task<Reservation> ReserveAndSaveAsync(
        CreateReservationRequestDto request, IReadOnlyCollection<int> equipmentIds, CancellationToken cancellationToken)
    {
        await _equipment.LockForShareAsync(equipmentIds, cancellationToken);

        // 1. Load the equipment (one query) and make sure every requested item exists and is active.
        var equipmentById = (await _equipment.GetActiveByIdsAsync(equipmentIds, cancellationToken)).ToDictionary(e => e.Id);
        var missingId = equipmentIds.FirstOrDefault(id => !equipmentById.ContainsKey(id));
        if (missingId != 0)
            throw new NotFoundException("Equipment", missingId);

        // 2. Reserve the stock for every item on every day, or fail with nothing changed.
        var stockRequests = request.Items.Select(i => new StockRequest(equipmentById[i.EquipmentId], i.Quantity)).ToList();
        await _stockAllocator.ReserveAsync(stockRequests, request.StartDate, request.EndDate, cancellationToken);

        // 3. Build the reservation, with the prices as they are right now.
        var days = DateRangeRules.DayCount(request.StartDate, request.EndDate);
        var reservation = new Reservation
        {
            UserId = _currentUser.Id,
            EventName = request.EventName.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = ReservationStatus.Confirmed,
            CreatedAtUtc = _timeProvider.GetUtcNow().UtcDateTime,
            Items = request.Items.Select(i => new ReservationItem
            {
                EquipmentId = i.EquipmentId,
                Quantity = i.Quantity,
                UnitPrice = equipmentById[i.EquipmentId].DailyPrice
            }).ToList()
        };
        reservation.TotalPrice = reservation.Items.Sum(item => item.UnitPrice * days * item.Quantity);
        await _reservations.AddAsync(reservation, cancellationToken);

        // 4. One save for everything (stock rows and the reservation): all or nothing.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return reservation;
    }

    public async Task<ReservationResponseDto> ChangeStatusAsync(int id, ReservationStatus newStatus, CancellationToken cancellationToken)
    {
        if (newStatus is not (ReservationStatus.Cancelled or ReservationStatus.Returned))
            throw new BusinessValidationException("The status can only be changed to Cancelled or Returned.");

        var reservation = await _reservations.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("Reservation", id);

        EnsureTransitionIsAllowed(reservation, newStatus);

        if (newStatus == ReservationStatus.Cancelled)
        {
            // Give the stock back on every day of the range, through the same code that reserved it.
            await ReleaseStockAsync(reservation, reservation.StartDate, reservation.EndDate, cancellationToken);
        }
        else
        {
            // Returned: remember the actual return day. If it is before the booked end, the equipment is free again
            // from the following day (dates have no time of day, so the return day itself stays reserved).
            var returnedOn = _clock.Today;
            reservation.ReturnedOn = returnedOn;
            if (returnedOn < reservation.EndDate)
                await ReleaseStockAsync(reservation, returnedOn.AddDays(1), reservation.EndDate, cancellationToken);
        }

        reservation.Status = newStatus;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            _logger.LogWarning(
                "Reservation conflict: user {UserId} lost a race changing reservation {ReservationId} ({StartDate} to {EndDate}) to {NewStatus}.",
                _currentUser.Id, reservation.Id, reservation.StartDate, reservation.EndDate, newStatus);
            throw;
        }

        _logger.LogInformation("Reservation {ReservationId} changed to {NewStatus} by user {UserId}.", reservation.Id, newStatus, _currentUser.Id);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<PagedResult<ReservationResponseDto>> GetPagedAsync(ReservationQuery query, CancellationToken cancellationToken)
    {
        int? onlyUserId = _currentUser.IsAdmin ? null : _currentUser.Id;
        var (items, totalCount) = await _reservations.GetPagedAsync(onlyUserId, query, cancellationToken);
        return new PagedResult<ReservationResponseDto>(
            _mapper.Map<IReadOnlyList<ReservationResponseDto>>(items), query.Page, query.PageSize, totalCount);
    }

    public async Task<ReservationResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var reservation = await _reservations.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Reservation", id);

        if (!_currentUser.IsAdmin && reservation.UserId != _currentUser.Id)
            throw new ForbiddenException("You can only view your own reservations.");

        return _mapper.Map<ReservationResponseDto>(reservation);
    }

    /// <summary>
    /// The status rules, in one place:
    /// Confirmed -> Cancelled (owner or admin, before the start date); Confirmed -> Returned (admin only, once it has started;
    /// returning before the end date frees the stock from the following day).
    /// Cancelled and Returned are final. Who may do it is checked first, then the current state and the dates.
    /// </summary>
    private void EnsureTransitionIsAllowed(Reservation reservation, ReservationStatus newStatus)
    {
        if (newStatus == ReservationStatus.Cancelled && !_currentUser.IsAdmin && reservation.UserId != _currentUser.Id)
            throw new ForbiddenException("You can only cancel your own reservations.");

        if (newStatus == ReservationStatus.Returned && !_currentUser.IsAdmin)
            throw new ForbiddenException("Only an admin can mark a reservation as returned.");

        if (reservation.Status != ReservationStatus.Confirmed)
            throw new ConflictException($"The reservation is already {reservation.Status} and can no longer be changed.");

        if (newStatus == ReservationStatus.Cancelled && _clock.Today >= reservation.StartDate)
            throw new ConflictException("A reservation can only be cancelled before its start date.");

        if (newStatus == ReservationStatus.Returned && _clock.Today < reservation.StartDate)
            throw new ConflictException("A reservation cannot be marked as returned before it starts. Cancel it instead.");
    }

    /// <summary>Gives back the reserved quantities of all the reservation's items on the days from <paramref name="from"/> to <paramref name="to"/>.</summary>
    private Task ReleaseStockAsync(Reservation reservation, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var stockRequests = reservation.Items.Select(i => new StockRequest(i.Equipment, i.Quantity)).ToList();
        return _stockAllocator.ReleaseAsync(stockRequests, from, to, cancellationToken);
    }

    /// <summary>Rules that do not need the database: the dates and the list of items.</summary>
    private void ValidateRequest(CreateReservationRequestDto request)
    {
        DateRangeRules.EnsureValid(request.StartDate, request.EndDate);

        if (request.StartDate < _clock.Today)
            throw new BusinessValidationException("The start date cannot be in the past.");

        if (request.Items.Count == 0)
            throw new BusinessValidationException("A reservation must contain at least one item.");

        if (request.Items.Select(i => i.EquipmentId).Distinct().Count() != request.Items.Count)
            throw new BusinessValidationException("Each equipment item can appear only once in a reservation.");
    }
}

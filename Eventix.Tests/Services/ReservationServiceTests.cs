using AutoMapper;
using Eventix.Application.DTOs.Reservations;
using Eventix.Application.Exceptions;
using Eventix.Application.Interfaces;
using Eventix.Application.Mapping;
using Eventix.Application.Services;
using Eventix.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Eventix.Tests.Services;

/// <summary>
/// Service-layer tests of the limited resource (equipment units per day), with mocked repositories.
/// The real <see cref="StockAllocator"/> runs on in-memory stock rows, because that is where the stock logic lives.
/// </summary>
public class ReservationServiceTests
{
    private static readonly DateOnly Start = new(2030, 6, 10);
    private static readonly DateOnly End = Start.AddDays(2);      // three days, both ends included
    private const int UserId = 7;

    private static readonly IMapper Mapper = CreateMapper();

    private readonly Equipment _microphone = new() { Id = 4, Name = "Wireless Handheld Microphone", DailyPrice = 40m, TotalQuantity = 12, IsActive = true };
    private readonly Equipment _followSpot = new() { Id = 9, Name = "Follow Spot", DailyPrice = 150m, TotalQuantity = 2, IsActive = true };

    private readonly Mock<IReservationRepository> _reservations = new();
    private readonly Mock<IEquipmentRepository> _equipment = new();
    private readonly Mock<IDailyStockRepository> _dailyStock = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IBusinessClock> _clock = new();
    private readonly List<DailyStock> _stockRows = new();
    private Reservation? _addedReservation;

    public ReservationServiceTests()
    {
        _currentUser.SetupGet(u => u.Id).Returns(UserId);
        _clock.SetupGet(c => c.Today).Returns(Start);

        _equipment
            .Setup(e => e.GetActiveByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<int> ids, CancellationToken _) =>
                (IReadOnlyList<Equipment>)new[] { _microphone, _followSpot }.Where(e => ids.Contains(e.Id)).ToList());

        _dailyStock
            .Setup(s => s.GetOrCreateForRangeAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<int> ids, DateOnly from, DateOnly to, CancellationToken _) =>
                (IReadOnlyList<DailyStock>)_stockRows.Where(r => ids.Contains(r.EquipmentId) && r.Date >= from && r.Date <= to).ToList());

        // The transaction wrapper simply runs the action, and saving succeeds.
        _unitOfWork
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task<Reservation>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task<Reservation>> action, CancellationToken _) => action());
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // "Saving" a reservation gives it an id and its navigation data, so the service can read it back and map it.
        _reservations
            .Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .Callback<Reservation, CancellationToken>((reservation, _) =>
            {
                reservation.Id = 100;
                reservation.User = new User { Id = UserId, FullName = "Test Producer" };
                foreach (var item in reservation.Items)
                    item.Equipment = new[] { _microphone, _followSpot }.Single(e => e.Id == item.EquipmentId);
                _addedReservation = reservation;
            })
            .Returns(Task.CompletedTask);
        _reservations
            .Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _addedReservation);
    }

    [Fact]
    public async Task Create_WithEnoughStock_ReservesEveryDayAndSavesOnce()
    {
        GivenStock(_microphone, 0, 0, 0);
        GivenStock(_followSpot, 0, 0, 0);

        var result = await CreateService().CreateAsync(Request((4, 3), (9, 1)), CancellationToken.None);

        Assert.Equal("Confirmed", result.Status);
        Assert.Equal(UserId, result.UserId);
        Assert.Equal(2, result.Items.Count);
        // (40 x 3 units + 150 x 1 unit) x 3 days
        Assert.Equal(810m, result.TotalPrice);
        Assert.Equal(40m, result.Items.Single(i => i.EquipmentId == 4).UnitPrice);

        // The stock was taken on all three days, for both items.
        Assert.All(RowsOf(_microphone), row => Assert.Equal(3, row.ReservedQuantity));
        Assert.All(RowsOf(_followSpot), row => Assert.Equal(1, row.ReservedQuantity));

        _reservations.Verify(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenStockIsFull_IsRejectedAndNothingIsSaved()
    {
        GivenStock(_microphone, 0, 0, 0);          // the microphones would fit...
        GivenStock(_followSpot, 2, 2, 2);          // ...but both follow spots are already taken on every day

        var exception = await Assert.ThrowsAsync<InsufficientStockException>(
            () => CreateService().CreateAsync(Request((4, 3), (9, 1)), CancellationToken.None));

        Assert.Equal(_followSpot.Name, exception.EquipmentName);
        Assert.Equal(Start, exception.Date);
        Assert.Equal(1, exception.Requested);
        Assert.Equal(0, exception.Available);

        // All or nothing: the microphones that did fit were not reserved either, and nothing was saved.
        Assert.All(RowsOf(_microphone), row => Assert.Equal(0, row.ReservedQuantity));
        _reservations.Verify(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>Creates the stock rows of one item for the three days, with the given already-reserved quantities.</summary>
    private void GivenStock(Equipment equipment, params int[] reservedPerDay)
    {
        for (var i = 0; i < reservedPerDay.Length; i++)
            _stockRows.Add(new DailyStock { EquipmentId = equipment.Id, Date = Start.AddDays(i), ReservedQuantity = reservedPerDay[i] });
    }

    private IEnumerable<DailyStock> RowsOf(Equipment equipment) => _stockRows.Where(r => r.EquipmentId == equipment.Id);

    private static CreateReservationRequestDto Request(params (int EquipmentId, int Quantity)[] items) => new()
    {
        EventName = "Summer festival",
        StartDate = Start,
        EndDate = End,
        Items = items.Select(i => new ReservationItemRequestDto { EquipmentId = i.EquipmentId, Quantity = i.Quantity }).ToList()
    };

    private ReservationService CreateService() => new(
        _reservations.Object,
        _equipment.Object,
        new StockAllocator(_dailyStock.Object),
        _unitOfWork.Object,
        _currentUser.Object,
        _clock.Object,
        new FixedTimeProvider(new DateTimeOffset(2030, 6, 1, 9, 0, 0, TimeSpan.Zero)),
        Mapper,
        NullLogger<ReservationService>.Instance);

    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfigurationExpression();
        configuration.AddProfile<MappingProfile>();
        return new MapperConfiguration(configuration).CreateMapper();
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

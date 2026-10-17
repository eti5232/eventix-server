using Eventix.Application.Exceptions;
using Eventix.Domain.Entities;
using Eventix.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Eventix.Tests.Concurrency;

/// <summary>
/// Proof of the optimistic concurrency on the contested resource (<see cref="DailyStock"/>): two separate DbContexts read
/// the same row, both change it, the first saves, and the second is rejected. Runs on SQLite in memory.
/// </summary>
public sealed class OptimisticConcurrencyTests : IDisposable
{
    private const int EquipmentId = 15;                 // exists in the seed data
    private static readonly DateOnly Day = new(2030, 6, 10);

    // One open connection keeps the in-memory database alive; every context below uses it.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public OptimisticConcurrencyTests()
    {
        _connection.Open();

        using var context = NewContext();
        context.Database.EnsureCreated();
        context.DailyStocks.Add(new DailyStock { EquipmentId = EquipmentId, Date = Day, ReservedQuantity = 0 });
        context.SaveChanges();
    }

    [Fact]
    public async Task SecondContext_SavingAStaleDailyStock_ThrowsDbUpdateConcurrencyException()
    {
        await using var first = NewContext();
        await using var second = NewContext();

        // Both read the same row, with the same Version.
        var firstCopy = await first.DailyStocks.SingleAsync();
        var secondCopy = await second.DailyStocks.SingleAsync();
        var originalVersion = firstCopy.Version;

        // Both change it.
        firstCopy.ReservedQuantity = 1;
        secondCopy.ReservedQuantity = 2;

        await first.SaveChangesAsync();                                                    // the first one wins
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());   // the second is rejected

        await using var check = NewContext();
        var stored = await check.DailyStocks.AsNoTracking().SingleAsync();
        Assert.Equal(1, stored.ReservedQuantity);                                          // the winner's value is in the database
        Assert.NotEqual(originalVersion, stored.Version);                                  // and the Version was replaced
    }

    [Fact]
    public async Task UnitOfWork_TurnsTheLostRaceIntoConcurrencyConflictException()
    {
        await using var first = NewContext();
        await using var second = NewContext();

        var firstCopy = await first.DailyStocks.SingleAsync();
        var secondCopy = await second.DailyStocks.SingleAsync();
        firstCopy.ReservedQuantity = 1;
        secondCopy.ReservedQuantity = 2;

        await new UnitOfWork(first).SaveChangesAsync(CancellationToken.None);

        // The API turns this exception into HTTP 409.
        await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => new UnitOfWork(second).SaveChangesAsync(CancellationToken.None));
    }

    private AppDbContext NewContext() => new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);

    public void Dispose() => _connection.Dispose();
}

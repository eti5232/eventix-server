using Eventix.Application.Exceptions;
using Eventix.Application.Interfaces;
using Eventix.Application.Services;
using Eventix.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Eventix.Infrastructure;

/// <summary>
/// Saves all pending changes of the DbContext in one transaction, and translates the database errors that mean
/// "someone else got there first" into <see cref="ConflictException"/>s (HTTP 409). Any other database error is not touched.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // The concurrency token (Version) changed since the row was read: another request updated the same row first.
            // Must come before the DbUpdateException clauses below, because it derives from DbUpdateException.
            throw new ConcurrencyConflictException();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception, DailyStockConfiguration.UniqueIndexName))
        {
            // Two requests tried to create the same (equipment, day) stock row; the unique index stopped the second one.
            throw new ConcurrencyConflictException();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception, UserConfiguration.UniqueEmailIndexName))
        {
            // Two registrations with the same email passed the "email exists" check together; the unique index stopped the second.
            throw new ConflictException(AuthService.EmailAlreadyRegisteredMessage);
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        // The SaveChanges calls inside the action join this transaction. If the action throws, leaving the "await using"
        // block without a commit rolls everything back.
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var result = await action();
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken) =>
        ExecuteInTransactionAsync<bool>(async () =>
        {
            await action();
            return true;
        }, cancellationToken);

    /// <summary>True only for PostgreSQL error 23505 (unique violation) on the given constraint.</summary>
    private static bool IsUniqueViolation(DbUpdateException exception, string constraintName) =>
        exception.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
        && postgresException.ConstraintName == constraintName;
}

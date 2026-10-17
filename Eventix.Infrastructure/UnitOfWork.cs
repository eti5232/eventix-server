using Eventix.Application.Exceptions;
using Eventix.Application.Interfaces;
using Eventix.Application.Services;
using Eventix.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Eventix.Infrastructure;

/// <summary>
/// Saves all pending changes of the DbContext in one transaction, and translates database errors that mean
/// "someone else got there first" into <see cref="ConflictException"/> (HTTP 409).
/// Stage 6 adds the concurrency conflicts of the daily stock.
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
        catch (DbUpdateException exception) when (IsUniqueViolation(exception, UserConfiguration.UniqueEmailIndexName))
        {
            // Two registrations with the same email passed the "email exists" check together; the unique index stopped the second.
            throw new ConflictException(AuthService.EmailAlreadyRegisteredMessage);
        }
    }

    /// <summary>True only for PostgreSQL error 23505 (unique violation) on the given constraint. Any other database error is not matched.</summary>
    private static bool IsUniqueViolation(DbUpdateException exception, string constraintName) =>
        exception.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
        && postgresException.ConstraintName == constraintName;
}

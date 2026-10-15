using Eventix.Application.Interfaces;

namespace Eventix.Infrastructure;

/// <summary>
/// Saves all pending changes of the DbContext in one transaction.
/// Stage 6 extends it to translate concurrency conflicts into <c>ConcurrencyConflictException</c>.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}

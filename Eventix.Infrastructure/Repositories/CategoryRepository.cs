using Eventix.Application.Interfaces;
using Eventix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eventix.Infrastructure.Repositories;

/// <summary>EF Core implementation of <see cref="ICategoryRepository"/>.</summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken) =>
        await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Category?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken) =>
        _context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) =>
        _context.Categories.AnyAsync(c => c.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLower();
        return _context.Categories.AnyAsync(
            c => c.Name.ToLower() == normalizedName && c.Id != excludeId, cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken) =>
        await _context.Categories.AddAsync(category, cancellationToken);
}

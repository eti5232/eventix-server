using Eventix.Application.Interfaces;
using Eventix.Domain.Entities;
using Eventix.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Eventix.Infrastructure.Seed;

/// <summary>
/// Creates the demo users at startup, so the system can be used right after the database is created.
/// It only adds users that are missing, so running it again never creates duplicates or changes existing users.
/// The categories and equipment are seeded by the migrations (<see cref="CatalogSeedData"/>), because they have fixed ids.
/// </summary>
public class DbSeeder
{
    private sealed record DemoUser(string FullName, string Email, string Password, UserRole Role);

    // Demo-only accounts; the same credentials are listed in the README for the reviewer.
    private static readonly DemoUser[] DemoUsers =
    {
        new("Demo Admin", "admin@eventix.dev", "Admin123!", UserRole.Admin),
        new("Demo Producer One", "producer1@eventix.dev", "Producer123!", UserRole.Producer),
        new("Demo Producer Two", "producer2@eventix.dev", "Producer123!", UserRole.Producer)
    };

    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(AppDbContext context, IPasswordHasher passwordHasher, TimeProvider timeProvider, ILogger<DbSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>Adds the demo users that do not exist yet.</summary>
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var existingEmails = await _context.Users.AsNoTracking().Select(u => u.Email).ToListAsync(cancellationToken);
        var missing = DemoUsers.Where(demo => !existingEmails.Contains(demo.Email)).ToList();

        if (missing.Count == 0)
            return;

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        foreach (var demo in missing)
        {
            _context.Users.Add(new User
            {
                FullName = demo.FullName,
                Email = demo.Email,
                PasswordHash = _passwordHasher.Hash(demo.Password),
                Role = demo.Role,
                CreatedAtUtc = now
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} demo user(s).", missing.Count);
    }
}

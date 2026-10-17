using Eventix.Application.Interfaces;
using Eventix.Application.Options;
using Eventix.Infrastructure.Auth;
using Eventix.Infrastructure.Repositories;
using Eventix.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Eventix.Infrastructure;

/// <summary>Registration of the infrastructure layer in the DI container.</summary>
public static class DependencyInjection
{
    /// <summary>Registers the DbContext (scoped) against PostgreSQL, using <c>ConnectionStrings:Default</c>.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' is missing. Set it with user-secrets or an environment variable.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        // Scoped, like the DbContext they depend on: one instance per HTTP request.
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<DbSeeder>();

        // Auth: stateless helpers can be singletons (they hold no DbContext). The JWT settings come from the "Jwt" section;
        // its Key is a secret and is supplied only through user-secrets or environment variables.
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }

    /// <summary>Applies pending migrations. Called once at startup.</summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>Creates the demo users if they are missing. Called once at startup, after the migrations.</summary>
    public static async Task SeedDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync(cancellationToken);
    }
}

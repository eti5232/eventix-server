using Eventix.Application.Interfaces;
using Eventix.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Eventix.Application;

/// <summary>Registration of the application layer in the DI container.</summary>
public static class DependencyInjection
{
    /// <summary>Registers AutoMapper profiles and the business services (scoped, like their repositories).</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStockAllocator, StockAllocator>();
        services.AddScoped<IReservationService, ReservationService>();

        return services;
    }
}

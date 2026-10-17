using Eventix.Application.Common;
using Eventix.Application.DTOs.Equipment;

namespace Eventix.Application.Interfaces;

/// <summary>Business logic for the equipment catalog.</summary>
public interface IEquipmentService
{
    Task<PagedResult<EquipmentResponseDto>> GetPagedAsync(EquipmentQuery query, CancellationToken cancellationToken);

    /// <exception cref="Exceptions.NotFoundException">The equipment does not exist.</exception>
    Task<EquipmentResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <exception cref="Exceptions.NotFoundException">The category does not exist.</exception>
    Task<EquipmentResponseDto> CreateAsync(EquipmentRequestDto request, CancellationToken cancellationToken);

    /// <exception cref="Exceptions.NotFoundException">The equipment or its category does not exist.</exception>
    /// <exception cref="Exceptions.ConflictException">The new total quantity is lower than what is already reserved on a future day.
    /// The check and the change run under a lock, so a reservation created at the same moment cannot slip past it.</exception>
    Task<EquipmentResponseDto> UpdateAsync(int id, EquipmentRequestDto request, CancellationToken cancellationToken);

    /// <summary>How many units are free on every day of the range (total minus the highest reserved quantity on any day of it).</summary>
    /// <exception cref="Exceptions.BusinessValidationException">The date range is reversed or longer than 7 days.</exception>
    /// <exception cref="Exceptions.NotFoundException">The equipment does not exist or was removed.</exception>
    Task<AvailabilityResponseDto> GetAvailabilityAsync(int id, DateOnly from, DateOnly to, CancellationToken cancellationToken);

    /// <summary>Soft delete: sets IsActive to false, the row is kept for past reservations.</summary>
    /// <exception cref="Exceptions.NotFoundException">The equipment does not exist.</exception>
    /// <exception cref="Exceptions.ConflictException">Confirmed reservations that contain it have not ended yet.</exception>
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}

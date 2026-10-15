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
    Task<EquipmentResponseDto> UpdateAsync(int id, EquipmentRequestDto request, CancellationToken cancellationToken);

    /// <summary>Soft delete: sets IsActive to false, the row is kept for past reservations.</summary>
    /// <exception cref="Exceptions.NotFoundException">The equipment does not exist.</exception>
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}

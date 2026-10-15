using System.ComponentModel.DataAnnotations;

namespace Eventix.Application.DTOs.Equipment;

/// <summary>Input for creating or updating an equipment item. Removal is done with DELETE, not through this DTO.</summary>
public class EquipmentRequestDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0.01, 99999999.99)]
    public decimal DailyPrice { get; set; }

    [Range(0, 100000)]
    public int TotalQuantity { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

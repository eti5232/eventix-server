namespace Eventix.Application.DTOs.Equipment;

/// <summary>An equipment item as returned by the API.</summary>
public class EquipmentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DailyPrice { get; set; }
    public int TotalQuantity { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

namespace Eventix.Application.DTOs.Equipment;

/// <summary>How many units of an equipment item are free on every day of a range (the minimum over the range).</summary>
public class AvailabilityResponseDto
{
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public int TotalQuantity { get; set; }
    public int AvailableQuantity { get; set; }
}

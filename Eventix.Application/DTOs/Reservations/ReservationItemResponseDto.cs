namespace Eventix.Application.DTOs.Reservations;

/// <summary>One line of a reservation. <see cref="UnitPrice"/> is the daily price at the time of booking.</summary>
public class ReservationItemResponseDto
{
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

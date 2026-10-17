using System.ComponentModel.DataAnnotations;

namespace Eventix.Application.DTOs.Reservations;

/// <summary>One equipment item in a new reservation.</summary>
public class ReservationItemRequestDto
{
    [Range(1, int.MaxValue)]
    public int EquipmentId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }
}

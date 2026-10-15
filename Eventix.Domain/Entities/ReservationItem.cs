namespace Eventix.Domain.Entities;

/// <summary>
/// Join entity of the many-to-many relation between reservations and equipment.
/// The key is (ReservationId, EquipmentId).
/// </summary>
public class ReservationItem
{
    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;
    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
    public int Quantity { get; set; }

    /// <summary>Daily price at the time of booking, so later price changes do not affect it.</summary>
    public decimal UnitPrice { get; set; }
}

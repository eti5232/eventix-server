using Eventix.Domain.Enums;

namespace Eventix.Domain.Entities;

/// <summary>A producer's booking of several equipment items for a date range (up to 7 days, inclusive).</summary>
public class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string EventName { get; set; } = string.Empty;
    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;

    /// <summary>Sum of unit price x days x quantity over all items.</summary>
    public decimal TotalPrice { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<ReservationItem> Items { get; set; } = new List<ReservationItem>();
}

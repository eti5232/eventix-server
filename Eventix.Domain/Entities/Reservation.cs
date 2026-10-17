using Eventix.Domain.Common;
using Eventix.Domain.Enums;

namespace Eventix.Domain.Entities;

/// <summary>
/// A producer's booking of several equipment items for a date range (up to 7 days, inclusive).
/// It carries a concurrency token, so two simultaneous status changes (for example cancel and return) cannot both succeed.
/// </summary>
public class Reservation : IVersioned
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

    /// <summary>
    /// The day the equipment was actually returned (set when the status becomes Returned). It can be before <see cref="EndDate"/>
    /// (an early return, which frees the stock from the following day) or after it (a late return).
    /// </summary>
    public DateOnly? ReturnedOn { get; set; }

    public Guid Version { get; set; } = Guid.NewGuid();

    public ICollection<ReservationItem> Items { get; set; } = new List<ReservationItem>();
}

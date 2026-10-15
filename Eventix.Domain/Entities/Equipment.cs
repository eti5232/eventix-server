namespace Eventix.Domain.Entities;

/// <summary>A rentable equipment item. <see cref="TotalQuantity"/> is the limited resource.</summary>
public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DailyPrice { get; set; }
    public int TotalQuantity { get; set; }

    /// <summary>False means soft-deleted: hidden from the catalog but kept for past reservations.</summary>
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<DailyStock> DailyStocks { get; set; } = new List<DailyStock>();
    public ICollection<ReservationItem> ReservationItems { get; set; } = new List<ReservationItem>();
}

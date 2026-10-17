namespace Eventix.Application.DTOs.Reservations;

/// <summary>A reservation as returned by the API.</summary>
public class ReservationResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;

    /// <summary>The day the equipment was actually returned; null until the reservation is marked as returned.</summary>
    public DateOnly? ReturnedOn { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<ReservationItemResponseDto> Items { get; set; } = new();
}

using System.ComponentModel.DataAnnotations;

namespace Eventix.Application.DTOs.Reservations;

/// <summary>Input for a new reservation: one date range (inclusive, at most 7 days) and one or more equipment items.</summary>
public class CreateReservationRequestDto
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string EventName { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    [Required, MinLength(1)]
    public List<ReservationItemRequestDto> Items { get; set; } = new();
}

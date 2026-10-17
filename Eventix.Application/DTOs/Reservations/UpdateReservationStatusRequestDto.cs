using Eventix.Domain.Enums;

namespace Eventix.Application.DTOs.Reservations;

/// <summary>Input for changing a reservation's status. Only Cancelled and Returned are valid targets.</summary>
public class UpdateReservationStatusRequestDto
{
    public ReservationStatus Status { get; set; }
}

namespace Eventix.Domain.Enums;

/// <summary>Lifecycle state of a reservation. Cancelled and Returned are final.</summary>
public enum ReservationStatus
{
    Confirmed,
    Cancelled,
    Returned
}

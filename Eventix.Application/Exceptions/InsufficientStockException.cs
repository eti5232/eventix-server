namespace Eventix.Application.Exceptions;

/// <summary>Thrown when an equipment item has fewer free units on some day than were requested. Translated to HTTP 409.</summary>
public class InsufficientStockException : ConflictException
{
    public InsufficientStockException(string equipmentName, DateOnly date, int requested, int available)
        : base($"Not enough '{equipmentName}' available on {date:yyyy-MM-dd}: requested {requested}, available {available}.")
    {
        EquipmentName = equipmentName;
        Date = date;
        Requested = requested;
        Available = available;
    }

    public string EquipmentName { get; }
    public DateOnly Date { get; }
    public int Requested { get; }
    public int Available { get; }
}

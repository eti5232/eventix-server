using Eventix.Domain.Common;

namespace Eventix.Domain.Entities;

/// <summary>
/// How many units of one equipment item are reserved on one date.
/// Users compete on this row, so it carries the concurrency token.
/// </summary>
public class DailyStock : IVersioned
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
    public DateOnly Date { get; set; }
    public int ReservedQuantity { get; set; }
    public Guid Version { get; set; } = Guid.NewGuid();
}

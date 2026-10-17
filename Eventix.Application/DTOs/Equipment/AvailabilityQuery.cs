using System.ComponentModel.DataAnnotations;

namespace Eventix.Application.DTOs.Equipment;

/// <summary>The date range of an availability question, from the query string (<c>?from=2026-10-20&amp;to=2026-10-22</c>).</summary>
public class AvailabilityQuery
{
    [Required]
    public DateOnly? From { get; set; }

    [Required]
    public DateOnly? To { get; set; }
}

using Eventix.Application.Exceptions;

namespace Eventix.Application.Common;

/// <summary>
/// The rules for a date range (a reservation, or an availability question): inclusive on both ends, at most 7 days.
/// Written once and used by every feature that takes a range.
/// </summary>
public static class DateRangeRules
{
    public const int MaxDays = 7;

    /// <exception cref="BusinessValidationException">The end is before the start, or the range is longer than <see cref="MaxDays"/> days.</exception>
    public static void EnsureValid(DateOnly start, DateOnly end)
    {
        if (end < start)
            throw new BusinessValidationException("The end date must be on or after the start date.");

        if (DayCount(start, end) > MaxDays)
            throw new BusinessValidationException($"The date range can span at most {MaxDays} days.");
    }

    /// <summary>Number of days in the range, counting both the start and the end day.</summary>
    public static int DayCount(DateOnly start, DateOnly end) => end.DayNumber - start.DayNumber + 1;

    /// <summary>Every day of the range, in order, including the start and the end day.</summary>
    public static IEnumerable<DateOnly> Days(DateOnly start, DateOnly end)
    {
        for (var day = start; day <= end; day = day.AddDays(1))
            yield return day;
    }
}

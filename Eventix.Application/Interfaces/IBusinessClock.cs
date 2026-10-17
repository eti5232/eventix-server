namespace Eventix.Application.Interfaces;

/// <summary>The business calendar: what "today" means for the customers (Israel time), decided in one place.</summary>
public interface IBusinessClock
{
    /// <summary>Today's date in the business time zone. It can differ from the UTC date for a few hours each day.</summary>
    DateOnly Today { get; }
}

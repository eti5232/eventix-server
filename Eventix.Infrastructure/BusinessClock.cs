using Eventix.Application.Interfaces;

namespace Eventix.Infrastructure;

/// <summary><see cref="IBusinessClock"/> based on <see cref="TimeProvider"/> and a configured time zone (never <c>DateTime.Now</c>).</summary>
public class BusinessClock : IBusinessClock
{
    private readonly TimeProvider _timeProvider;
    private readonly TimeZoneInfo _timeZone;

    public BusinessClock(TimeProvider timeProvider, TimeZoneInfo timeZone)
    {
        _timeProvider = timeProvider;
        _timeZone = timeZone;
    }

    public DateOnly Today =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), _timeZone).DateTime);
}

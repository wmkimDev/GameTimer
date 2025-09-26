namespace GameTimer.Schedules.Schedules;

using System.Collections.Generic;
using GameTimer.Abstractions;
using Common;
using Models;

internal sealed class LocalOneTimeSchedule : LocalScheduleBase
{
    private readonly ScheduleWindow _window;

    public LocalOneTimeSchedule(
        IClock clock,
        TimeZoneInfo timeZone,
        DstPolicy policy,
        DateTime startLocal,
        DateTime endLocal) : base(clock, timeZone, policy)
    {
        if (startLocal.Kind == DateTimeKind.Utc || endLocal.Kind == DateTimeKind.Utc)
            throw new ArgumentException("Local schedule requires local DateTime values.");

        var startUtc = DstHelpers.SafeConvertToUtc(DateTime.SpecifyKind(startLocal, DateTimeKind.Unspecified), ScheduleTimeZone, Policy);
        var endUtc   = DstHelpers.SafeConvertToUtc(DateTime.SpecifyKind(endLocal, DateTimeKind.Unspecified), ScheduleTimeZone, Policy);

        _window = new ScheduleWindow(startUtc, endUtc);
    }

    protected override ScheduleWindow? FindCurrentWindow(DateTime referenceUtc)
    {
        return _window.Contains(referenceUtc) ? _window : null;
    }

    protected override ScheduleWindow? FindNextWindow(DateTime referenceUtc)
    {
        if (referenceUtc < _window.StartUtc)
            return _window;

        return null;
    }

    protected override IEnumerable<ScheduleWindow> IterateWindows(DateTime fromUtc, DateTime toUtc)
    {
        if (_window.Intersects(fromUtc, toUtc))
            yield return _window;
    }
}

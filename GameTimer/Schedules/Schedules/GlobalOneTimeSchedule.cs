namespace GameTimer.Schedules.Schedules;

using System.Collections.Generic;
using GameTimer.Abstractions;
using Models;

internal sealed class GlobalOneTimeSchedule : GlobalScheduleBase
{
    private readonly ScheduleWindow _window;

    public GlobalOneTimeSchedule(IClock clock, DateTime startUtc, DateTime endUtc) : base(clock)
    {
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

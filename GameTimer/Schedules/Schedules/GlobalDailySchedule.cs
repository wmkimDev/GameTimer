namespace GameTimer.Schedules.Schedules;

using System;
using System.Collections.Generic;
using GameTimer.Abstractions;
using Models;
using Internal;

internal sealed class GlobalDailySchedule : GlobalScheduleBase
{
    private readonly DailyWindowDefinition[] _windows;

    public GlobalDailySchedule(IClock clock, IEnumerable<DailyWindowDefinition> windows) : base(clock)
    {
        if (windows == null) throw new ArgumentNullException(nameof(windows));
        _windows = ScheduleOrdering.SortDailyWindows(windows);
        if (_windows.Length == 0)
            throw new ArgumentException("At least one daily window must be provided.", nameof(windows));
    }

    protected override ScheduleWindow? FindCurrentWindow(DateTime referenceUtc)
    {
        var baseDate = referenceUtc.Date;
        for (var offset = -1; offset <= 1; offset++)
        {
            var day = baseDate.AddDays(offset);
            foreach (var window in _windows)
            {
                var startUtc = ScheduleMath.CreateUtcDate(day, window.Start);
                var endUtc   = startUtc + window.Duration;

                if (referenceUtc >= startUtc && referenceUtc < endUtc)
                    return new ScheduleWindow(startUtc, endUtc);
            }
        }

        return null;
    }

    protected override ScheduleWindow? FindNextWindow(DateTime referenceUtc)
    {
        var baseDate = referenceUtc.Date;

        for (var offset = 0; offset <= 2; offset++)
        {
            var day = baseDate.AddDays(offset);
            foreach (var window in _windows)
            {
                var startUtc = ScheduleMath.CreateUtcDate(day, window.Start);
                var endUtc   = startUtc + window.Duration;

                if (startUtc <= referenceUtc)
                    continue;

                return new ScheduleWindow(startUtc, endUtc);
            }
        }

        return null;
    }

    protected override IEnumerable<ScheduleWindow> IterateWindows(DateTime fromUtc, DateTime toUtc)
    {
        if (_windows.Length == 0)
            yield break;

        var startDate = fromUtc.Date.AddDays(-1);
        var endDate   = toUtc.Date.AddDays(1);

        for (var day = startDate; day <= endDate; day = day.AddDays(1))
        {
            foreach (var window in _windows)
            {
                var startUtc = ScheduleMath.CreateUtcDate(day, window.Start);
                var endUtc   = startUtc + window.Duration;

                if (endUtc <= fromUtc || startUtc >= toUtc)
                    continue;

                yield return new ScheduleWindow(startUtc, endUtc);
            }
        }
    }
}

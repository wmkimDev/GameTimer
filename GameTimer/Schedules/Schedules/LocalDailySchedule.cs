namespace GameTimer.Schedules.Schedules;

using System;
using System.Collections.Generic;
using GameTimer.Abstractions;
using Common;
using Internal;
using Models;

internal sealed class LocalDailySchedule : LocalScheduleBase
{
    private readonly DailyWindowDefinition[] _windows;

    public LocalDailySchedule(
        IClock clock,
        TimeZoneInfo timeZone,
        DstPolicy policy,
        IEnumerable<DailyWindowDefinition> windows) : base(clock, timeZone, policy)
    {
        if (windows == null) throw new ArgumentNullException(nameof(windows));
        _windows = ScheduleOrdering.SortDailyWindows(windows);
        if (_windows.Length == 0)
            throw new ArgumentException("At least one daily window must be provided.", nameof(windows));
    }

    protected override IEnumerable<ScheduleWindow> IterateWindows(DateTime fromUtc, DateTime toUtc)
    {
        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, ScheduleTimeZone).Date.AddDays(-1);
        var endLocal   = TimeZoneInfo.ConvertTimeFromUtc(toUtc, ScheduleTimeZone).Date.AddDays(1);

        for (var day = startLocal; day <= endLocal; day = day.AddDays(1))
        {
            foreach (var window in _windows)
            {
                var localStart = ScheduleMath.CreateLocalDate(day, window.Start);
                var startUtc   = DstHelpers.SafeConvertToUtc(localStart, ScheduleTimeZone, Policy);
                var localEnd   = localStart.Add(window.Duration);
                var endUtc     = DstHelpers.SafeConvertToUtc(localEnd, ScheduleTimeZone, Policy);

                if (endUtc <= fromUtc || startUtc >= toUtc)
                    continue;

                yield return new ScheduleWindow(startUtc, endUtc);
            }
        }
    }

    protected override ScheduleWindow? FindCurrentWindow(DateTime referenceUtc)
    {
        var localReference = TimeZoneInfo.ConvertTimeFromUtc(referenceUtc, ScheduleTimeZone);
        var baseDate = localReference.Date;

        for (var offset = -1; offset <= 1; offset++)
        {
            var day = baseDate.AddDays(offset);
            foreach (var window in _windows)
            {
                var localStart = ScheduleMath.CreateLocalDate(day, window.Start);
                var startUtc   = DstHelpers.SafeConvertToUtc(localStart, ScheduleTimeZone, Policy);
                var localEnd   = localStart.Add(window.Duration);
                var endUtc     = DstHelpers.SafeConvertToUtc(localEnd, ScheduleTimeZone, Policy);

                if (referenceUtc >= startUtc && referenceUtc < endUtc)
                    return new ScheduleWindow(startUtc, endUtc);
            }
        }

        return null;
    }

    protected override ScheduleWindow? FindNextWindow(DateTime referenceUtc)
    {
        var localReference = TimeZoneInfo.ConvertTimeFromUtc(referenceUtc, ScheduleTimeZone);
        var baseDate = localReference.Date;

        for (var offset = 0; offset <= 2; offset++)
        {
            var day = baseDate.AddDays(offset);
            foreach (var window in _windows)
            {
                var localStart = ScheduleMath.CreateLocalDate(day, window.Start);
                var startUtc   = DstHelpers.SafeConvertToUtc(localStart, ScheduleTimeZone, Policy);
                var localEnd   = localStart.Add(window.Duration);
                var endUtc     = DstHelpers.SafeConvertToUtc(localEnd, ScheduleTimeZone, Policy);

                if (startUtc <= referenceUtc)
                    continue;

                return new ScheduleWindow(startUtc, endUtc);
            }
        }

        return null;
    }
}

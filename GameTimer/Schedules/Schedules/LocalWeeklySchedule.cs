namespace GameTimer.Schedules.Schedules;

using System;
using System.Collections.Generic;
using GameTimer.Abstractions;
using Common;
using Internal;
using Models;

internal sealed class LocalWeeklySchedule : LocalScheduleBase
{
    private readonly WeeklyWindowDefinition[] _definitions;

    public LocalWeeklySchedule(
        IClock clock,
        TimeZoneInfo timeZone,
        DstPolicy policy,
        IEnumerable<WeeklyWindowDefinition> definitions) : base(clock, timeZone, policy)
    {
        if (definitions == null) throw new ArgumentNullException(nameof(definitions));
        _definitions = ScheduleOrdering.SortWeeklyDefinitions(definitions);
        if (_definitions.Length == 0)
            throw new ArgumentException("At least one weekly window must be provided.", nameof(definitions));
    }

    protected override IEnumerable<ScheduleWindow> IterateWindows(DateTime fromUtc, DateTime toUtc)
    {
        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, ScheduleTimeZone).Date.AddDays(-7);
        var endLocal   = TimeZoneInfo.ConvertTimeFromUtc(toUtc, ScheduleTimeZone).Date.AddDays(7);

        for (var day = startLocal; day <= endLocal; day = day.AddDays(1))
        {
            var flag = day.DayOfWeek.ToFlag();

            foreach (var def in _definitions)
            {
                if (!def.Days.HasFlag(flag))
                    continue;

                var localStart = ScheduleMath.CreateLocalDate(day, def.Start);
                var startUtc   = DstHelpers.SafeConvertToUtc(localStart, ScheduleTimeZone, Policy);
                var localEnd   = localStart.Add(def.Duration);
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

        for (var offset = -7; offset <= 0; offset++)
        {
            var day = baseDate.AddDays(offset);
            var flag = day.DayOfWeek.ToFlag();

            foreach (var def in _definitions)
            {
                if (!def.Days.HasFlag(flag))
                    continue;

                var localStart = ScheduleMath.CreateLocalDate(day, def.Start);
                var startUtc   = DstHelpers.SafeConvertToUtc(localStart, ScheduleTimeZone, Policy);
                var localEnd   = localStart.Add(def.Duration);
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

        for (var offset = 0; offset <= 14; offset++)
        {
            var day = baseDate.AddDays(offset);
            var flag = day.DayOfWeek.ToFlag();

            foreach (var def in _definitions)
            {
                if (!def.Days.HasFlag(flag))
                    continue;

                var localStart = ScheduleMath.CreateLocalDate(day, def.Start);
                var startUtc   = DstHelpers.SafeConvertToUtc(localStart, ScheduleTimeZone, Policy);
                var localEnd   = localStart.Add(def.Duration);
                var endUtc     = DstHelpers.SafeConvertToUtc(localEnd, ScheduleTimeZone, Policy);

                if (startUtc <= referenceUtc)
                    continue;

                return new ScheduleWindow(startUtc, endUtc);
            }
        }

        return null;
    }
}

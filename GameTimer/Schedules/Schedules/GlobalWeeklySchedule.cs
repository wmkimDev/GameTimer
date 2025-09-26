namespace GameTimer.Schedules.Schedules;

using System;
using System.Collections.Generic;
using GameTimer.Abstractions;
using Common;
using Internal;
using Models;

internal sealed class GlobalWeeklySchedule : GlobalScheduleBase
{
    private readonly WeeklyWindowDefinition[] _definitions;

    public GlobalWeeklySchedule(IClock clock, IEnumerable<WeeklyWindowDefinition> definitions) : base(clock)
    {
        if (definitions == null) throw new ArgumentNullException(nameof(definitions));
        _definitions = ScheduleOrdering.SortWeeklyDefinitions(definitions);
        if (_definitions.Length == 0)
            throw new ArgumentException("At least one weekly window must be provided.", nameof(definitions));
    }

    protected override ScheduleWindow? FindCurrentWindow(DateTime referenceUtc)
    {
        var baseDate = referenceUtc.Date;
        for (var offset = -7; offset <= 0; offset++)
        {
            var day = baseDate.AddDays(offset);
            var flag = day.DayOfWeek.ToFlag();

            foreach (var def in _definitions)
            {
                if (!def.Days.HasFlag(flag))
                    continue;

                var startUtc = ScheduleMath.CreateUtcDate(day, def.Start);
                var endUtc   = startUtc + def.Duration;

                if (referenceUtc >= startUtc && referenceUtc < endUtc)
                    return new ScheduleWindow(startUtc, endUtc);
            }
        }

        return null;
    }

    protected override ScheduleWindow? FindNextWindow(DateTime referenceUtc)
    {
        var baseDate = referenceUtc.Date;
        for (var offset = 0; offset <= 14; offset++)
        {
            var day = baseDate.AddDays(offset);
            var flag = day.DayOfWeek.ToFlag();

            foreach (var def in _definitions)
            {
                if (!def.Days.HasFlag(flag))
                    continue;

                var startUtc = ScheduleMath.CreateUtcDate(day, def.Start);
                var endUtc   = startUtc + def.Duration;

                if (startUtc <= referenceUtc)
                    continue;

                return new ScheduleWindow(startUtc, endUtc);
            }
        }

        return null;
    }

    protected override IEnumerable<ScheduleWindow> IterateWindows(DateTime fromUtc, DateTime toUtc)
    {
        var startDate = fromUtc.Date.AddDays(-7);
        var endDate   = toUtc.Date.AddDays(7);

        for (var day = startDate; day <= endDate; day = day.AddDays(1))
        {
            var flag = day.DayOfWeek.ToFlag();

            foreach (var def in _definitions)
            {
                if (!def.Days.HasFlag(flag))
                    continue;

                var startUtc = ScheduleMath.CreateUtcDate(day, def.Start);
                var endUtc   = startUtc + def.Duration;

                if (endUtc <= fromUtc || startUtc >= toUtc)
                    continue;

                yield return new ScheduleWindow(startUtc, endUtc);
            }
        }
    }
}

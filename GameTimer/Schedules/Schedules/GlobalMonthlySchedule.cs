using GameTimer.Schedules.Internal;

namespace GameTimer.Schedules.Schedules;

using System;
using System.Collections.Generic;
using GameTimer.Abstractions;
using Models;

internal sealed class GlobalMonthlySchedule : GlobalScheduleBase
{
    private readonly MonthlyWindowDefinition[] _definitions;

    public GlobalMonthlySchedule(IClock clock, IEnumerable<MonthlyWindowDefinition> definitions) : base(clock)
    {
        if (definitions == null) throw new ArgumentNullException(nameof(definitions));
        _definitions = ScheduleOrdering.SortMonthlyDefinitions(definitions);
        if (_definitions.Length == 0)
            throw new ArgumentException("At least one monthly window must be provided.", nameof(definitions));
    }

    protected override ScheduleWindow? FindCurrentWindow(DateTime referenceUtc)
    {
        var cursor = new DateTime(referenceUtc.Year, referenceUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
        for (int i = 0; i < 3; i++)
        {
            foreach (var def in _definitions)
            {
                var window = CreateWindow(cursor, def);
                if (!window.HasValue)
                    continue;
                var w = window.Value;
                if (referenceUtc >= w.StartUtc && referenceUtc < w.EndUtc)
                    return w;
            }

            cursor = cursor.AddMonths(1);
        }

        return null;
    }

    protected override ScheduleWindow? FindNextWindow(DateTime referenceUtc)
    {
        var cursor = new DateTime(referenceUtc.Year, referenceUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        for (int i = 0; i < 24; i++)
        {
            foreach (var def in _definitions)
            {
                var window = CreateWindow(cursor, def);
                if (!window.HasValue)
                    continue;

                var w = window.Value;
                if (w.StartUtc <= referenceUtc)
                    continue;

                return w;
            }

            cursor = cursor.AddMonths(1);
        }

        return null;
    }

    protected override IEnumerable<ScheduleWindow> IterateWindows(DateTime fromUtc, DateTime toUtc)
    {
        var monthCursor = new DateTime(fromUtc.Year, fromUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
        var limit       = new DateTime(toUtc.Year, toUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(2);

        while (monthCursor < limit)
        {
            foreach (var def in _definitions)
            {
                var window = CreateWindow(monthCursor, def);
                if (!window.HasValue)
                    continue;

                var w = window.Value;

                if (w.EndUtc <= fromUtc || w.StartUtc >= toUtc)
                    continue;

                yield return w;
            }

            monthCursor = monthCursor.AddMonths(1);
        }
    }

    private ScheduleWindow? CreateWindow(DateTime monthCursor, MonthlyWindowDefinition def)
    {
        var targetDay = def.DayOfMonth == MonthlyWindowDefinition.LastDay
            ? DateTime.DaysInMonth(monthCursor.Year, monthCursor.Month)
            : Math.Min(def.DayOfMonth, DateTime.DaysInMonth(monthCursor.Year, monthCursor.Month));

        var startUtc = new DateTime(
            monthCursor.Year,
            monthCursor.Month,
            targetDay,
            def.Start.Hour,
            def.Start.Minute,
            def.Start.Second,
            def.Start.Millisecond,
            DateTimeKind.Utc);

        var endUtc = startUtc + def.Duration;

        if (endUtc <= startUtc)
            return null;

        return new ScheduleWindow(startUtc, endUtc);
    }
}

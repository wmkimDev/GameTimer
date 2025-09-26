using GameTimer.Schedules.Internal;

namespace GameTimer.Schedules.Schedules;

using System;
using System.Collections.Generic;
using GameTimer.Abstractions;
using Common;
using Models;

internal sealed class LocalMonthlySchedule : LocalScheduleBase
{
    private readonly MonthlyWindowDefinition[] _definitions;

    public LocalMonthlySchedule(
        IClock clock,
        TimeZoneInfo timeZone,
        DstPolicy policy,
        IEnumerable<MonthlyWindowDefinition> definitions) : base(clock, timeZone, policy)
    {
        if (definitions == null) throw new ArgumentNullException(nameof(definitions));
        _definitions = ScheduleOrdering.SortMonthlyDefinitions(definitions);
        if (_definitions.Length == 0)
            throw new ArgumentException("At least one monthly window must be provided.", nameof(definitions));
    }

    protected override IEnumerable<ScheduleWindow> IterateWindows(DateTime fromUtc, DateTime toUtc)
    {
        var localFrom = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, ScheduleTimeZone);
        var localTo   = TimeZoneInfo.ConvertTimeFromUtc(toUtc, ScheduleTimeZone);

        var cursor = new DateTime(localFrom.Year, localFrom.Month, 1, 0, 0, 0, DateTimeKind.Unspecified).AddMonths(-1);
        var limit  = new DateTime(localTo.Year, localTo.Month, 1, 0, 0, 0, DateTimeKind.Unspecified).AddMonths(2);

        while (cursor < limit)
        {
            foreach (var def in _definitions)
            {
                var window = CreateWindow(cursor, def);
                if (!window.HasValue)
                    continue;

                var w = window.Value;
                if (w.EndUtc <= fromUtc || w.StartUtc >= toUtc)
                    continue;

                yield return w;
            }

            cursor = cursor.AddMonths(1);
        }
    }

    protected override ScheduleWindow? FindCurrentWindow(DateTime referenceUtc)
    {
        var localReference = TimeZoneInfo.ConvertTimeFromUtc(referenceUtc, ScheduleTimeZone);
        var cursor = new DateTime(localReference.Year, localReference.Month, 1, 0, 0, 0, DateTimeKind.Unspecified).AddMonths(-1);

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
        var localReference = TimeZoneInfo.ConvertTimeFromUtc(referenceUtc, ScheduleTimeZone);
        var cursor = new DateTime(localReference.Year, localReference.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);

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

    private ScheduleWindow? CreateWindow(DateTime monthCursorLocal, MonthlyWindowDefinition def)
    {
        var daysInMonth = DateTime.DaysInMonth(monthCursorLocal.Year, monthCursorLocal.Month);
        var targetDay   = def.DayOfMonth == MonthlyWindowDefinition.LastDay
            ? daysInMonth
            : Math.Min(def.DayOfMonth, daysInMonth);

        var localStart = new DateTime(
            monthCursorLocal.Year,
            monthCursorLocal.Month,
            targetDay,
            def.Start.Hour,
            def.Start.Minute,
            def.Start.Second,
            def.Start.Millisecond,
            DateTimeKind.Unspecified);

        var startUtc = DstHelpers.SafeConvertToUtc(localStart, ScheduleTimeZone, Policy);
        var endUtc   = DstHelpers.SafeConvertToUtc(localStart.Add(def.Duration), ScheduleTimeZone, Policy);

        if (endUtc <= startUtc)
            return null;

        return new ScheduleWindow(startUtc, endUtc);
    }
}

namespace GameTimer.Schedules.Schedules;

using System;
using System.Collections.Generic;
using GameTimer.Abstractions;
using Common;
using Abstractions;
using Models;

internal abstract class ScheduleBase : ISchedule
{
    private readonly IClock _clock;

    protected ScheduleBase(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public ScheduleSnapshot GetSnapshot()
    {
        return GetSnapshot(_clock.UtcNow);
    }

    public ScheduleSnapshot GetSnapshot(DateTime utcNow)
    {
        TimerHelpers.ValidateUtc(utcNow, nameof(utcNow));

        var current = FindCurrentWindow(utcNow);
        var next    = FindNextWindow(utcNow);

        var timeUntilStart = current.HasValue
            ? TimeSpan.Zero
            : next.HasValue ? next.Value.StartUtc - utcNow : TimeSpan.MaxValue;

        var timeUntilEnd = current.HasValue
            ? current.Value.EndUtc - utcNow
            : TimeSpan.Zero;

        return new ScheduleSnapshot(
            isActive: current.HasValue,
            timeUntilStart: timeUntilStart,
            timeUntilEnd: timeUntilEnd,
            currentWindow: current,
            nextWindow: next);
    }

    public ScheduleWindow? GetNextWindow()
    {
        return GetNextWindow(_clock.UtcNow);
    }

    public ScheduleWindow? GetNextWindow(DateTime lastWindowUtc)
    {
        TimerHelpers.ValidateUtc(lastWindowUtc, nameof(lastWindowUtc));
        return FindNextWindow(lastWindowUtc);
    }

    public IEnumerable<ScheduleWindow> EnumerateWindows(DateTime fromUtc, DateTime toUtc)
    {
        TimerHelpers.ValidateUtc(fromUtc, nameof(fromUtc));
        TimerHelpers.ValidateUtc(toUtc, nameof(toUtc));
        if (toUtc <= fromUtc)
            throw new ArgumentException("toUtc must be after fromUtc", nameof(toUtc));

        return EnumerateInternal(fromUtc, toUtc);
    }

    IEnumerable<ScheduleWindow> ISchedule.EnumerateWindows(DateTime fromUtc, DateTime toUtc)
        => EnumerateWindows(fromUtc, toUtc);

    protected abstract ScheduleWindow? FindCurrentWindow(DateTime referenceUtc);

    protected abstract ScheduleWindow? FindNextWindow(DateTime referenceUtc);

    protected abstract IEnumerable<ScheduleWindow> IterateWindows(DateTime fromUtc, DateTime toUtc);

    public abstract TimeZoneInfo? TimeZone { get; }

    private IEnumerable<ScheduleWindow> EnumerateInternal(DateTime fromUtc, DateTime toUtc)
    {
        foreach (var window in IterateWindows(fromUtc, toUtc))
        {
            if (window.Intersects(fromUtc, toUtc))
                yield return window;
        }
    }
}

namespace GameTimer.Schedules.Models;

using System;
using Common;

public readonly struct WeeklyWindowDefinition
{
    public WeeklyWindowDefinition(DayOfWeekFlag days, TimeOfDay start, TimeSpan duration)
    {
        if (days == DayOfWeekFlag.None)
            throw new ArgumentException("At least one day must be specified.", nameof(days));
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");

        Days     = days;
        Start    = start;
        Duration = duration;
    }

    public DayOfWeekFlag Days { get; }
    public TimeOfDay Start { get; }
    public TimeSpan Duration { get; }
}

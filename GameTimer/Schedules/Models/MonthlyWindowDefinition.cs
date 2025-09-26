namespace GameTimer.Schedules.Models;

using System;
using Common;

public readonly struct MonthlyWindowDefinition
{
    public const int LastDay = -1;

    public MonthlyWindowDefinition(int dayOfMonth, TimeOfDay start, TimeSpan duration)
    {
        if (dayOfMonth == 0 || dayOfMonth < LastDay || dayOfMonth > 31)
            throw new ArgumentOutOfRangeException(nameof(dayOfMonth), "Day must be 1-31 or LastDay (-1).");
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");

        DayOfMonth = dayOfMonth;
        Start      = start;
        Duration   = duration;
    }

    public int DayOfMonth { get; }
    public TimeOfDay Start { get; }
    public TimeSpan Duration { get; }
}

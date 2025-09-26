namespace GameTimer.Schedules.Models;

using System;
using Common;

public readonly struct DailyWindowDefinition
{
    public DailyWindowDefinition(TimeOfDay start, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");

        Start    = start;
        Duration = duration;
    }

    public TimeOfDay Start { get; }

    public TimeSpan Duration { get; }
}

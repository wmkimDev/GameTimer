namespace GameTimer.Schedules.Models;

using System;

/// <summary>
/// UTC 기준 활성 구간.
/// </summary>
public readonly struct ScheduleWindow
{
    public ScheduleWindow(DateTime startUtc, DateTime endUtc)
    {
        if (startUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Start must be UTC", nameof(startUtc));
        if (endUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("End must be UTC", nameof(endUtc));
        if (endUtc <= startUtc)
            throw new ArgumentException("End must be after start", nameof(endUtc));

        StartUtc = startUtc;
        EndUtc   = endUtc;
    }

    public DateTime StartUtc { get; }
    public DateTime EndUtc   { get; }

    public TimeSpan Duration => EndUtc - StartUtc;

    public bool Contains(DateTime utcInstant)
    {
        if (utcInstant.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Value must be UTC", nameof(utcInstant));

        return utcInstant >= StartUtc && utcInstant < EndUtc;
    }

    public bool Intersects(DateTime fromUtc, DateTime toUtc)
    {
        if (fromUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("fromUtc must be UTC", nameof(fromUtc));
        if (toUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("toUtc must be UTC", nameof(toUtc));
        if (toUtc <= fromUtc)
            throw new ArgumentException("toUtc must be after fromUtc", nameof(toUtc));

        return StartUtc < toUtc && EndUtc > fromUtc;
    }
}

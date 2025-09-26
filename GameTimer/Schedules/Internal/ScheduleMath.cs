namespace GameTimer.Schedules.Internal;

using System;
using Common;

internal static class ScheduleMath
{
    public static DateTime CreateUtcDate(DateTime dayUtc, TimeOfDay time)
    {
        return new DateTime(
            dayUtc.Year,
            dayUtc.Month,
            dayUtc.Day,
            time.Hour,
            time.Minute,
            time.Second,
            time.Millisecond,
            DateTimeKind.Utc);
    }

    public static DateTime CreateLocalDate(DateTime dayLocal, TimeOfDay time)
    {
        return new DateTime(
            dayLocal.Year,
            dayLocal.Month,
            dayLocal.Day,
            time.Hour,
            time.Minute,
            time.Second,
            time.Millisecond,
            DateTimeKind.Unspecified);
    }
}

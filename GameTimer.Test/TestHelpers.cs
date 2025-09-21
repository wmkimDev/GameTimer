using System;
using GameTimer.Clocks;

namespace GameTimer.Test;

internal static class TestHelpers
{
    public static DateTime Utc(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
        => new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

    public static DateTime Unspec(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
        => new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);

    public static DateTime ToUtcLocal(DateTime localUnspec, TimeZoneInfo tz)
        => TimeZoneInfo.ConvertTimeToUtc(localUnspec, tz);

    public static FixedClock ClockUtc(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
        => new FixedClock(Utc(year, month, day, hour, minute, second));

    public static TimeZoneInfo Tz(params string[] ids)
    {
        foreach (var id in ids)
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch { /* try next */ }
        }
        
        // Fallback mappings for common zones (IANA -> Windows)
        var map = new (string iana, string windows)[]
        {
            ("Asia/Seoul", "Korea Standard Time"),
            ("America/Los_Angeles", "Pacific Standard Time"),
            ("Europe/Berlin", "W. Europe Standard Time"),
            ("UTC", "UTC"),
        };
        
        foreach (var (iana, windows) in map)
        {
            foreach (var id in new[] { iana, windows })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch { }
            }
        }
        throw new InvalidOperationException("Could not resolve a test time zone on this system.");
    }
}


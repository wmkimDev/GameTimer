using System;
using GameTimer.Clocks;
using GameTimer.Common;
using GameTimer.Schedules.Builders;
using Shouldly;

namespace GameTimer.Test;

public class ScheduleOrderingTests
{
    private static readonly TimeZoneInfo UtcZone = TimeZoneInfo.Utc;

    [Fact]
    public void LocalDailySchedule_UsesSortedWindows()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1, 0, 0, 0));
        var schedule = LocalSchedules
            .Daily(clock, UtcZone)
            .AddWindow(18, 0, TimeSpan.FromHours(1))
            .AddWindow(9, 0, TimeSpan.FromHours(1))
            .Build();

        var reference = TestHelpers.Utc(2025, 1, 1, 8, 0, 0);
        var next = schedule.GetNextWindow(reference);

        next.ShouldNotBeNull();
        var window = next.Value;
        window.StartUtc.ShouldBe(TestHelpers.Utc(2025, 1, 1, 9, 0, 0));
    }

    [Fact]
    public void LocalWeeklySchedule_SortsByStartTimePerDay()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 5, 0, 0, 0));
        var schedule = LocalSchedules
            .Weekly(clock, UtcZone)
            .AddWindow(DayOfWeekFlag.Monday, 18, 0, TimeSpan.FromHours(1))
            .AddWindow(DayOfWeekFlag.Monday, 9, 0, TimeSpan.FromHours(1))
            .Build();

        var reference = TestHelpers.Utc(2025, 1, 6, 8, 0, 0); // Monday 08:00
        var next = schedule.GetNextWindow(reference);

        next.ShouldNotBeNull();
        var window = next.Value;
        window.StartUtc.ShouldBe(TestHelpers.Utc(2025, 1, 6, 9, 0, 0));
    }

    [Fact]
    public void LocalMonthlySchedule_SortsByDayThenStart()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1, 0, 0, 0));
        var schedule = LocalSchedules
            .Monthly(clock, UtcZone)
            .AddWindow(20, 8, 0, TimeSpan.FromHours(1))
            .AddWindow(5, 12, 0, TimeSpan.FromHours(1))
            .Build();

        var reference = TestHelpers.Utc(2025, 1, 1, 0, 0, 0);
        var next = schedule.GetNextWindow(reference);

        next.ShouldNotBeNull();
        var window = next.Value;
        window.StartUtc.ShouldBe(TestHelpers.Utc(2025, 1, 5, 12, 0, 0));
    }

    [Fact]
    public void GlobalDailySchedule_UsesSortedWindows()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1, 0, 0, 0));
        var schedule = GlobalSchedules
            .Daily(clock)
            .AddWindow(18, 0, TimeSpan.FromHours(1))
            .AddWindow(9, 0, TimeSpan.FromHours(1))
            .Build();

        var reference = TestHelpers.Utc(2025, 1, 1, 8, 0, 0);
        var next = schedule.GetNextWindow(reference);

        next.ShouldNotBeNull();
        var window = next.Value;
        window.StartUtc.ShouldBe(TestHelpers.Utc(2025, 1, 1, 9, 0, 0));
    }

    [Fact]
    public void GlobalWeeklySchedule_SortsByStartTimePerDay()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 5, 0, 0, 0));
        var schedule = GlobalSchedules
            .Weekly(clock)
            .AddWindow(DayOfWeekFlag.Monday, 18, 0, TimeSpan.FromHours(1))
            .AddWindow(DayOfWeekFlag.Monday, 9, 0, TimeSpan.FromHours(1))
            .Build();

        var reference = TestHelpers.Utc(2025, 1, 6, 8, 0, 0);
        var next = schedule.GetNextWindow(reference);

        next.ShouldNotBeNull();
        var window = next.Value;
        window.StartUtc.ShouldBe(TestHelpers.Utc(2025, 1, 6, 9, 0, 0));
    }

    [Fact]
    public void GlobalMonthlySchedule_SortsByDayThenStart()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1, 0, 0, 0));
        var schedule = GlobalSchedules
            .Monthly(clock)
            .AddWindow(20, 8, 0, TimeSpan.FromHours(1))
            .AddWindow(5, 12, 0, TimeSpan.FromHours(1))
            .Build();

        var reference = TestHelpers.Utc(2025, 1, 1, 0, 0, 0);
        var next = schedule.GetNextWindow(reference);

        next.ShouldNotBeNull();
        var window = next.Value;
        window.StartUtc.ShouldBe(TestHelpers.Utc(2025, 1, 5, 12, 0, 0));
    }
}

using System;
using System.Linq;
using GameTimer.Clocks;
using GameTimer.Common;
using GameTimer.Schedules.Builders;
using GameTimer.Schedules.Models;
using Shouldly;

namespace GameTimer.Test;

public class ScheduleEdgeCaseTests
{
    private static readonly TimeZoneInfo SeoulTz = TestHelpers.Tz("Asia/Seoul", "Korea Standard Time");
    private static readonly TimeZoneInfo LaTz    = TestHelpers.Tz("America/Los_Angeles", "Pacific Standard Time");

    [Fact]
    public void LocalOneTime_SpringForward_NextValidAdjustsStart()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 3, 9, 6, 0, 0));
        var builder = LocalSchedules.Once(clock, LaTz, DstPolicy.NextValid)
            .Between(TestHelpers.Unspec(2025, 3, 9, 2, 30, 0), TestHelpers.Unspec(2025, 3, 9, 3, 30, 0));

        var schedule = builder.Build();
        var next = schedule.GetNextWindow(TestHelpers.Utc(2025, 3, 9, 7, 0, 0));

        next.ShouldNotBeNull();
        var window = next.Value;
        window.StartUtc.ShouldBe(TestHelpers.Utc(2025, 3, 9, 10, 0, 0));
        window.EndUtc.ShouldBe(TestHelpers.Utc(2025, 3, 9, 10, 30, 0));
    }

    [Fact]
    public void LocalOneTime_SpringForward_ThrowException()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 3, 9, 6, 0, 0));
        var builder = LocalSchedules.Once(clock, LaTz, DstPolicy.ThrowException)
            .Between(TestHelpers.Unspec(2025, 3, 9, 2, 30, 0), TestHelpers.Unspec(2025, 3, 9, 3, 30, 0));

        Should.Throw<InvalidTimeZoneException>(() => builder.Build());
    }

    [Fact]
    public void LocalDaily_EnumerateWindows_IncludesOverlaps()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1, 0, 0, 0));
        var schedule = LocalSchedules
            .Daily(clock, TimeZoneInfo.Utc)
            .AddWindow(9, 0, 0, TimeSpan.FromHours(2))
            .AddWindow(10, 0, 0, TimeSpan.FromHours(1))
            .Build();

        var windows = schedule.EnumerateWindows(TestHelpers.Utc(2025, 1, 1, 8, 0, 0), TestHelpers.Utc(2025, 1, 1, 12, 0, 0)).ToArray();

        windows.Length.ShouldBe(2);
        windows[0].StartUtc.ShouldBe(TestHelpers.Utc(2025, 1, 1, 9, 0, 0));
        windows[1].StartUtc.ShouldBe(TestHelpers.Utc(2025, 1, 1, 10, 0, 0));
    }

    [Fact]
    public void LocalMonthly_LastDayAcrossDifferentMonthLengths()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1));
        var schedule = LocalSchedules
            .Monthly(clock, SeoulTz)
            .LastDay().At(8, 0, 0)
            .Build();

        var fromUtc = TestHelpers.Utc(2025, 1, 1);
        var toUtc   = TestHelpers.Utc(2025, 4, 1);
        var windows = schedule.EnumerateWindows(fromUtc, toUtc).ToArray();

        windows.Length.ShouldBe(3);

        var expectedLocals = new[]
        {
            TestHelpers.Unspec(2025, 1, 31, 8, 0, 0),
            TestHelpers.Unspec(2025, 2, 28, 8, 0, 0),
            TestHelpers.Unspec(2025, 3, 31, 8, 0, 0)
        };

        for (var i = 0; i < windows.Length; i++)
        {
            var expectedUtc = TimeZoneInfo.ConvertTimeToUtc(expectedLocals[i], SeoulTz);
            windows[i].StartUtc.ShouldBe(expectedUtc);
        }
    }

    [Fact]
    public void GlobalDailyBuilder_WithoutWindows_Throws()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1));
        Should.Throw<ArgumentException>(() => GlobalSchedules.Daily(clock).Build());
    }

    [Fact]
    public void GlobalWeekly_EnumerateWindows_SpansMultipleDays()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1));
        var schedule = GlobalSchedules
            .Weekly(clock)
            .AddWindow(DayOfWeekFlag.Monday, new TimeOfDay(9, 0, 0), TimeSpan.FromHours(1))
            .AddWindow(DayOfWeekFlag.Wednesday, new TimeOfDay(15, 0, 0), TimeSpan.FromHours(2))
            .Build();

        var fromUtc = TestHelpers.Utc(2025, 1, 5); // Sunday
        var toUtc   = TestHelpers.Utc(2025, 1, 9); // Thursday, span past Wednesday window
        var windows = schedule.EnumerateWindows(fromUtc, toUtc).ToArray();

        windows.Length.ShouldBe(2);
        windows[0].StartUtc.ShouldBe(TestHelpers.Utc(2025, 1, 6, 9, 0, 0));
        windows[1].StartUtc.ShouldBe(TestHelpers.Utc(2025, 1, 8, 15, 0, 0));
    }
}

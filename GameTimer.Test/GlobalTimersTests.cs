using GameTimer.Builder;
using GameTimer.Clocks;
using GameTimer.Common;
using GameTimer.Timers;
using Shouldly;

namespace GameTimer.Test;

public class GlobalTimersTests
{
    // 글로벌 매일 타이머: 같은 날 지정 시각을 지났다면 다음날 00:00으로 넘어가는지 검증
    [Fact]
    public void GlobalDaily_NextReset_WrapsToNextDay()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1, 0, 0, 1));
        var timer = GlobalTimers.Daily(clock).UtcAt(0).Build();

        var last  = TestHelpers.Utc(2025, 1, 1, 0, 0, 1);
        var next  = timer.NextResetUtc(last);

        next.ShouldBe(TestHelpers.Utc(2025, 1, 2, 0, 0, 0));
    }

    // 글로벌 매주 타이머: 지정 요일(월/수) 중 다음으로 일치하는 날을 찾는지 검증
    [Fact]
    public void GlobalWeekly_NextReset_FindsNextMatchingDay()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 5, 23, 0, 0)); // 2025-01-05 is Sunday
        var timer = GlobalTimers.Weekly(clock)
            .On(DayOfWeekFlag.Monday | DayOfWeekFlag.Wednesday)
            .UtcAt(1)
            .Build();

        var last = TestHelpers.Utc(2025, 1, 5, 23, 0, 0); // Sunday late
        var next = timer.NextResetUtc(last);

        next.ShouldBe(TestHelpers.Utc(2025, 1, 6, 1, 0, 0)); // Monday 01:00 UTC
    }

    // 글로벌 매월 타이머: 31일 설정이 31일이 없는 달(2월 등)에서 말일로 보정되는지 검증
    [Fact]
    public void GlobalMonthly_NextReset_HandlesShortMonth()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 2, 1, 0, 0, 0));
        var timer = GlobalTimers.Monthly(clock)
            .On(31)
            .UtcAt(0)
            .Build();

        var last = TestHelpers.Utc(2025, 2, 1, 0, 0, 0);
        var next = timer.NextResetUtc(last);

        // 2025-02 has 28 days
        next.ShouldBe(TestHelpers.Utc(2025, 2, 28, 0, 0, 0));
    }

    // Interval 타이머: 다음 리셋이 last + interval 인지 검증
    [Fact]
    public void IntervalTimer_AddsInterval()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1, 0, 0, 0));
        var timer = new IntervalTimer(clock, TimeSpan.FromHours(2));

        var last = TestHelpers.Utc(2025, 1, 1, 3, 15, 0);
        timer.NextResetUtc(last).ShouldBe(TestHelpers.Utc(2025, 1, 1, 5, 15, 0));
    }

    // AfterDuration 타이머: 다음 리셋이 last + duration 인지 검증
    [Fact]
    public void AfterDuration_AddsDuration()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1, 0, 0, 0));
        var timer = new AfterDurationTimer(clock, TimeSpan.FromMinutes(30));

        var last = TestHelpers.Utc(2025, 1, 1, 0, 10, 0);
        timer.NextResetUtc(last).ShouldBe(TestHelpers.Utc(2025, 1, 1, 0, 40, 0));
    }

    // OnceAt 타이머: Latency 버퍼로 목표시각 직전에도 ShouldReset 이 true 인지 검증
    [Fact]
    public void GlobalOnceAt_ShouldRespectLatencyForShouldReset()
    {
        var clock  = new FixedClock(TestHelpers.Utc(2025, 1, 1, 0, 0, 9));
        var target = TestHelpers.Utc(2025, 1, 1, 0, 0, 10);
        var timer  = new GlobalOnceAtTimer(clock, target);

        var last = TestHelpers.Utc(2024, 12, 31, 0, 0, 0);
        // Latency default 2s: now(9) >= target(10) - 2 => true
        timer.ShouldReset(last).ShouldBeTrue();
    }
}

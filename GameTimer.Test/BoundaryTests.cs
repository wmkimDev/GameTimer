using GameTimer.Builder;
using GameTimer.Clocks;
using GameTimer.Common;
using GameTimer.Timers;
using Shouldly;

namespace GameTimer.Test;

public class BoundaryTests
{
    // 글로벌 매일 타이머: Latency 경계 직전/직후 ShouldReset 동작 검증
    [Fact]
    public void GlobalDaily_ShouldReset_LatencyBoundary()
    {
        var last   = TestHelpers.Utc(2025, 1, 1, 0, 0, 0);
        var clock1 = new FixedClock(TestHelpers.Utc(2025, 1, 1, 23, 59, 57)); // 경계-1초 → false
        var clock2 = new FixedClock(TestHelpers.Utc(2025, 1, 1, 23, 59, 59)); // 경계+1초 → true (Latency=2s)

        var timer1 = GlobalTimers.Daily(clock1).UtcAt(0).Build();
        var timer2 = GlobalTimers.Daily(clock2).UtcAt(0).Build();

        timer1.ShouldReset(last).ShouldBeFalse();
        timer2.ShouldReset(last).ShouldBeTrue();
    }

    // 글로벌 다회 타이머: 같은 날 마지막 시각과 동일한 last인 경우, 다음 날 첫 시각으로 넘어감
    [Fact]
    public void GlobalMultipleTimes_EqualToLastTime_MovesToNextDayFirst()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1, 16, 0, 0));
        var timer = GlobalTimers.MultipleTimes(clock).UtcAtHours(0, 8, 16).Build();

        var last = TestHelpers.Utc(2025, 1, 1, 16, 0, 0);
        var next = timer.NextResetUtc(last);

        next.ShouldBe(TestHelpers.Utc(2025, 1, 2, 0, 0, 0));
    }

    // 로컬 주간 타이머: 같은 날 지정 시각과 동일하면 다음 주로 넘어감
    [Fact]
    public void LocalWeekly_EqualToResetTime_GoesToNextWeek()
    {
        var tz    = TestHelpers.Tz("Asia/Seoul", "Korea Standard Time");
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 6, 0, 0, 0)); // 월요일
        var timer = LocalTimers.Weekly(clock).On(DayOfWeekFlag.Monday).At(6, 0, 0).Build();

        var lastLocal = TestHelpers.Unspec(2025, 1, 6, 6, 0, 0); // 월요일 06:00 정확히
        var lastUtc   = TimeZoneInfo.ConvertTimeToUtc(lastLocal, tz);

        var nextUtc   = timer.NextResetUtc(lastUtc, tz);
        var nextLocal = TimeZoneInfo.ConvertTimeFromUtc(nextUtc, tz);

        nextLocal.Date.ShouldBe(TestHelpers.Unspec(2025, 1, 13).Date); // 다음 주 월요일
        nextLocal.Hour.ShouldBe(6);
        nextLocal.Minute.ShouldBe(0);
    }

    // 로컬 월간 타이머: 말일(LastDay)에서 그 시각을 지난 경우 다음 달 말일로 이동
    [Fact]
    public void LocalMonthly_LastDay_AfterResetTime_MovesToNextMonthLastDay()
    {
        var tz    = TestHelpers.Tz("Europe/Berlin", "W. Europe Standard Time");
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 31, 23, 0, 0));
        var timer = LocalTimers.Monthly(clock).LastDay().At(6, 0, 0).Build();

        // 1월 말일 07:00(리셋 06:00을 지난 후)
        var lastLocal = TestHelpers.Unspec(2025, 1, 31, 7, 0, 0);
        var lastUtc   = TimeZoneInfo.ConvertTimeToUtc(lastLocal, tz);

        var nextUtc   = timer.NextResetUtc(lastUtc, tz);
        var nextLocal = TimeZoneInfo.ConvertTimeFromUtc(nextUtc, tz);

        nextLocal.Hour.ShouldBe(6);
        nextLocal.Minute.ShouldBe(0);
        // 다음 달 말일인지 확인 (2월 말일)
        nextLocal.Month.ShouldBe(2);
        // 일(day)은 해당 달의 말일이어야 함
        nextLocal.Day.ShouldBe(DateTime.DaysInMonth(nextLocal.Year, nextLocal.Month));
    }

    // 로컬 매일 타이머: DST 가을 반복(모호 시각)에서 NextValid는 정상 변환, ThrowException은 예외
    [Fact]
    public void LocalDaily_DST_FallBack_AmbiguousTime_Behavior()
    {
        var tz    = TestHelpers.Tz("America/Los_Angeles", "Pacific Standard Time");
        // 가을 반복: 2025-11-02 01:30 (모호)
        var lastLocal = TestHelpers.Unspec(2025, 11, 2, 0, 0, 0);
        var lastUtc   = TimeZoneInfo.ConvertTimeToUtc(lastLocal, tz);

        // NextValid: 예외 없이 동작, 01:30 로컬에 해당하는 시각으로 변환 가능
        var clockOk = new FixedClock(TestHelpers.Utc(2025, 11, 2, 0, 0, 0));
        var ok      = LocalTimers.Daily(clockOk, DstPolicy.NextValid).At(1, 30).Build();

        var nextUtcOk   = ok.NextResetUtc(lastUtc, tz);
        var nextLocalOk = TimeZoneInfo.ConvertTimeFromUtc(nextUtcOk, tz);
        nextLocalOk.Hour.ShouldBe(1);
        nextLocalOk.Minute.ShouldBe(30);

        // ThrowException: 모호 시각에 대해 예외 발생
        var clockNg = new FixedClock(TestHelpers.Utc(2025, 11, 2, 0, 0, 0));
        var ng      = LocalTimers.Daily(clockNg, DstPolicy.ThrowException).At(1, 30).Build();
        Should.Throw<ArgumentException>(() => ng.NextResetUtc(lastUtc, tz));
    }

    // 유효성 검증: UTC가 아닌 lastUtc 전달 시 예외
    [Fact]
    public void Helpers_ValidateUtc_ThrowsOnNonUtc()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1));
        var timer = GlobalTimers.Daily(clock).UtcAt(0).Build();
        var nonUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        Should.Throw<ArgumentException>(() => timer.NextResetUtc(nonUtc));
    }

    // Interval/AfterDuration: 0 또는 음수 인자 시 예외
    [Fact]
    public void Interval_AfterDuration_InvalidArguments()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1));
        Should.Throw<ArgumentException>(() => new IntervalTimer(clock, TimeSpan.Zero));
        Should.Throw<ArgumentException>(() => new IntervalTimer(clock, TimeSpan.FromSeconds(-1)));
        Should.Throw<ArgumentException>(() => new AfterDurationTimer(clock, TimeSpan.Zero));
        Should.Throw<ArgumentException>(() => new AfterDurationTimer(clock, TimeSpan.FromSeconds(-1)));
    }
}


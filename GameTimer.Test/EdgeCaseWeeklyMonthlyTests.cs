using GameTimer.Builder;
using GameTimer.Clocks;
using GameTimer.Common;
using GameTimer.Timers;
using Shouldly;

namespace GameTimer.Test;

public class EdgeCaseWeeklyMonthlyTests
{
    // 글로벌 매주 타이머: 연말 경계(12/31 -> 01/01)에서 다음 요일/시각 계산
    [Fact]
    public void GlobalWeekly_CrossYearBoundary()
    {
        // 2029-12-31 23:59 UTC (월요일이라고 가정하지 않아도 요일 플래그만 확인)
        var last  = TestHelpers.Utc(2029, 12, 31, 23, 59, 0);
        var clock = new FixedClock(last);
        var timer = GlobalTimers.Weekly(clock)
            .On(DayOfWeekFlag.Tuesday | DayOfWeekFlag.Thursday)
            .UtcAt(0, 0, 0)
            .Build();

        var next = timer.NextResetUtc(last);
        // 다음 날 00:00 (새해, 화요일 00:00)
        next.ShouldBe(TestHelpers.Utc(2030, 1, 1, 0, 0, 0));
    }

    // 글로벌 매월 타이머: LastDay 팩토리로 달마다 말일 시각이 정확한지 검증
    [Fact]
    public void GlobalMonthly_LastDay_Factory_Correct()
    {
        var clockJan = new FixedClock(TestHelpers.Utc(2025, 1, 15));
        var timer    = GlobalTimers.Monthly(clockJan).LastDay().UtcAt(6).Build();

        var lastJan  = TestHelpers.Utc(2025, 1, 31, 7, 0, 0); // 1월 말일 지나서
        var nextFeb  = timer.NextResetUtc(lastJan);
        nextFeb.ShouldBe(TestHelpers.Utc(2025, 2, DateTime.DaysInMonth(2025, 2), 6, 0, 0));
    }

    // 로컬 매주 타이머: DST 봄 점프(일요일 02:30 미존재) - NextValid/ThrowException 정책 검증
    [Fact]
    public void LocalWeekly_DST_SpringForward_SundayAtNonexistentTime()
    {
        var tz    = TestHelpers.Tz("America/Los_Angeles", "Pacific Standard Time");
        var clock = new FixedClock(TestHelpers.Utc(2025, 3, 8, 0, 0, 0));

        // NextValid: 일요일 02:30이 존재하지 않는 날 → 다음 유효 시각으로 보정
        var ok = LocalTimers.Weekly(clock, DstPolicy.NextValid)
            .On(DayOfWeekFlag.Sunday)
            .At(2, 30)
            .Build();

        var lastLocal = TestHelpers.Unspec(2025, 3, 9, 0, 0, 0); // 일요일 00:00
        var lastUtc   = TimeZoneInfo.ConvertTimeToUtc(lastLocal, tz);

        var nextUtcOk   = ok.NextResetUtc(lastUtc, tz);
        var nextLocalOk = TimeZoneInfo.ConvertTimeFromUtc(nextUtcOk, tz);
        nextLocalOk.DayOfWeek.ShouldBe(DayOfWeek.Sunday);
        // 02:30 nonexistent -> 대개 03:00로 보정됨
        nextLocalOk.Hour.ShouldBe(3);
        nextLocalOk.Minute.ShouldBe(0);

        // ThrowException: 예외 발생
        var ng = LocalTimers.Weekly(clock, DstPolicy.ThrowException)
            .On(DayOfWeekFlag.Sunday)
            .At(2, 30)
            .Build();

        Should.Throw<InvalidTimeZoneException>(() => ng.NextResetUtc(lastUtc, tz));
    }

    // 로컬 매월 타이머: 31일 지정이 30일 달(4월)에서는 30일로 보정
    [Fact]
    public void LocalMonthly_Day31_OnThirtyDayMonth_AdjustsToLastDay()
    {
        var tz    = TestHelpers.Tz("Asia/Seoul", "Korea Standard Time");
        var clock = new FixedClock(TestHelpers.Utc(2025, 4, 1, 0, 0, 0));
        var timer = LocalTimers.Monthly(clock).On(31).At(6, 0, 0).Build();

        // 4월 1일 기준, 4월은 30일까지 → 4/30 06:00 이 정답
        var lastLocal = TestHelpers.Unspec(2025, 4, 1, 0, 0, 0);
        var lastUtc   = TimeZoneInfo.ConvertTimeToUtc(lastLocal, tz);

        var nextUtc   = timer.NextResetUtc(lastUtc, tz);
        var nextLocal = TimeZoneInfo.ConvertTimeFromUtc(nextUtc, tz);
        nextLocal.Month.ShouldBe(4);
        nextLocal.Day.ShouldBe(30);
        nextLocal.Hour.ShouldBe(6);
        nextLocal.Minute.ShouldBe(0);
    }

    // 로컬 다회 타이머: 입력 시각이 정렬되지 않아도 다음 시각 선택이 정확한지
    [Fact]
    public void LocalMultipleTimes_UnorderedInput_SelectsNextCorrectly()
    {
        var tz    = TestHelpers.Tz("Asia/Seoul", "Korea Standard Time");
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1, 0, 0, 0));
        var timer = LocalTimers.MultipleTimes(clock)
            .AtHours(18, 6, 12) // 뒤죽박죽 순서
            .Build();

        var lastLocal = TestHelpers.Unspec(2025, 1, 1, 7, 0, 0);
        var lastUtc   = TimeZoneInfo.ConvertTimeToUtc(lastLocal, tz);

        var nextUtc   = timer.NextResetUtc(lastUtc, tz);
        var nextLocal = TimeZoneInfo.ConvertTimeFromUtc(nextUtc, tz);
        nextLocal.Hour.ShouldBe(12);
        nextLocal.Minute.ShouldBe(0);
    }

    // 로컬 주간 타이머: 복수 요일(월/금)에서 직후 요일을 정확히 선택
    [Fact]
    public void LocalWeekly_MultipleDays_PicksNearestNext()
    {
        var tz    = TestHelpers.Tz("Europe/Berlin", "W. Europe Standard Time");
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 9, 12, 0, 0)); // 목요일
        var timer = LocalTimers.Weekly(clock).On(DayOfWeekFlag.Monday | DayOfWeekFlag.Friday).At(8, 0, 0).Build();

        var lastLocal = TestHelpers.Unspec(2025, 1, 9, 9, 0, 0); // 목 09:00
        var lastUtc   = TimeZoneInfo.ConvertTimeToUtc(lastLocal, tz);

        var nextUtc   = timer.NextResetUtc(lastUtc, tz);
        var nextLocal = TimeZoneInfo.ConvertTimeFromUtc(nextUtc, tz);
        nextLocal.DayOfWeek.ShouldBe(DayOfWeek.Friday);
        nextLocal.Hour.ShouldBe(8);
    }

    // 로컬 월간 타이머: 지정 시각과 정확히 같으면 다음 달로 넘어감
    [Fact]
    public void LocalMonthly_EqualToResetTime_GoesToNextMonth()
    {
        var tz    = TestHelpers.Tz("Asia/Seoul", "Korea Standard Time");
        var clock = new FixedClock(TestHelpers.Utc(2025, 5, 1, 0, 0, 0));
        var timer = LocalTimers.Monthly(clock).On(10).At(6, 0, 0).Build();

        var lastLocal = TestHelpers.Unspec(2025, 5, 10, 6, 0, 0); // 정확히 리셋 시각
        var lastUtc   = TimeZoneInfo.ConvertTimeToUtc(lastLocal, tz);

        var nextUtc   = timer.NextResetUtc(lastUtc, tz);
        var nextLocal = TimeZoneInfo.ConvertTimeFromUtc(nextUtc, tz);
        nextLocal.Month.ShouldBe(6);
        nextLocal.Day.ShouldBe(10);
        nextLocal.Hour.ShouldBe(6);
    }

    // 로컬 매일 타이머: Latency 경계 검증
    [Fact]
    public void LocalDaily_ShouldReset_LatencyBoundary()
    {
        var tz    = TestHelpers.Tz("Asia/Seoul", "Korea Standard Time");
        var timerClockBefore = new FixedClock(TestHelpers.Utc(2025, 1, 1, 20, 59, 57)); // KST=+9 → 06:00 KST 기준 경계 직전
        var timerClockAfter  = new FixedClock(TestHelpers.Utc(2025, 1, 1, 20, 59, 59)); // 경계 직후

        // KST(+9) 기준 06:00은 전일 21:00 UTC에 해당합니다.
        // timerClockBefore: 경계 3초 전 → ShouldReset=false
        // timerClockAfter:  경계 1초 전(Latency=2s) → ShouldReset=true
        var t1 = LocalTimers.Daily(timerClockBefore).At(6, 0, 0).Build();
        var t2 = LocalTimers.Daily(timerClockAfter).At(6, 0, 0).Build();

        // lastLocal은 로컬 날짜 2025-01-02 자정으로 설정 → 같은 날 06:00 KST가 다음 리셋
        var lastLocal = TestHelpers.Unspec(2025, 1, 2, 0, 0, 0);
        var lastUtc   = TimeZoneInfo.ConvertTimeToUtc(lastLocal, tz);

        t1.ShouldReset(lastUtc, tz).ShouldBeFalse();
        t2.ShouldReset(lastUtc, tz).ShouldBeTrue();
    }
}

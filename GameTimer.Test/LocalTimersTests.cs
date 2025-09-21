using GameTimer.Builder;
using GameTimer.Clocks;
using GameTimer.Common;
using GameTimer.Timers;
using Shouldly;

namespace GameTimer.Test;

public class LocalTimersTests
{
    // 로컬 매일 타이머: 같은 날 지정 시각(06:00) 이전이면 그 시각을, 지났다면 다음날로 넘어가는지 검증
    [Fact]
    public void LocalDaily_NextReset_NormalDay()
    {
        var tz    = TestHelpers.Tz("Asia/Seoul", "Korea Standard Time");
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1, 0, 0, 0));
        var timer = LocalTimers.Daily(clock).At(6).Build(); // 06:00 local

        var lastLocal = TestHelpers.Unspec(2025, 1, 1, 5, 30, 0);
        var lastUtc   = TestHelpers.ToUtcLocal(lastLocal, tz);

        var expectedLocal = TestHelpers.Unspec(2025, 1, 1, 6, 0, 0);
        var expectedUtc   = TestHelpers.ToUtcLocal(expectedLocal, tz);

        var next = timer.NextResetUtc(lastUtc, tz);
        next.ShouldBe(expectedUtc);
    }

    // 로컬 매일 타이머: DST 봄 점프(존재하지 않는 시각, 예: 02:30)에서 ThrowException 정책 시 예외 발생 검증
    [Fact]
    public void LocalDaily_DST_SpringForward_ThrowException()
    {
        // U.S. Pacific: 2025-03-09 02:00 -> 03:00 (02:xx nonexistent)
        var tz    = TestHelpers.Tz("America/Los_Angeles", "Pacific Standard Time");
        var clock = new FixedClock(TestHelpers.Utc(2025, 3, 8, 0, 0, 0));
        var timer = LocalTimers.Daily(clock, DstPolicy.ThrowException).At(2, 30).Build();

        var lastLocal = TestHelpers.Unspec(2025, 3, 9, 0, 0, 0); // same day midnight local
        var lastUtc   = TestHelpers.ToUtcLocal(lastLocal, tz);

        Should.Throw<InvalidTimeZoneException>(() => timer.NextResetUtc(lastUtc, tz));
    }

    // 로컬 매일 타이머: DST 봄 점프에서 NextValid 정책으로 다음 유효 시각(보통 03:00)으로 보정되는지 검증
    [Fact]
    public void LocalDaily_DST_SpringForward_NextValid()
    {
        var tz    = TestHelpers.Tz("America/Los_Angeles", "Pacific Standard Time");
        var clock = new FixedClock(TestHelpers.Utc(2025, 3, 8, 0, 0, 0));
        var timer = LocalTimers.Daily(clock, DstPolicy.NextValid).At(2, 30).Build();

        var lastLocal = TestHelpers.Unspec(2025, 3, 9, 0, 0, 0);
        var lastUtc   = TestHelpers.ToUtcLocal(lastLocal, tz);

        var nextUtc   = timer.NextResetUtc(lastUtc, tz);
        var nextLocal = TimeZoneInfo.ConvertTimeFromUtc(nextUtc, tz);

        // 02:30 nonexistent -> next valid typically 03:00 local
        nextLocal.Hour.ShouldBe(3);
        nextLocal.Minute.ShouldBe(0);
    }

    // LocalOnceAt 타이머: UTC DateTime 을 전달하면 예외가 발생하는지 검증
    [Fact]
    public void LocalOnceAt_RejectsUtcInput()
    {
        var clock = new FixedClock(TestHelpers.Utc(2025, 1, 1, 0, 0, 0));
        var utc   = new DateTime(2025, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        Should.Throw<ArgumentException>(() => new LocalOnceAtTimer(clock, utc));
    }
}

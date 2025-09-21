using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

public sealed class GlobalOnceAtTimer : GlobalTimerBase
{
    private readonly DateTime _targetTimeUtc;

    /// <summary>
    /// 특정 UTC 시점에 한 번만 실행되는 글로벌 타이머 생성
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="targetTimeUtc">실행될 UTC 시점</param>
    /// <exception cref="ArgumentException">targetTimeUtc가 UTC가 아닌 경우</exception>
    public GlobalOnceAtTimer(IClock clock, DateTime targetTimeUtc) : base(clock)
    {
        TimerHelpers.ValidateUtc(targetTimeUtc, nameof(targetTimeUtc));
        _targetTimeUtc = targetTimeUtc;
    }

    /// <summary>
    /// 편의 생성자 - 날짜와 시각을 개별 매개변수로 설정
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="year">년도</param>
    /// <param name="month">월 (1-12)</param>
    /// <param name="day">일 (1-31)</param>
    /// <param name="hour">UTC 시간 (0-23)</param>
    /// <param name="minute">UTC 분 (0-59)</param>
    /// <param name="second">UTC 초 (0-59)</param>
    public GlobalOnceAtTimer(IClock clock, int year, int month, int day, 
        int hour = 0, int minute = 0, int second = 0)
        : this(clock, new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc))
    {
    }

    public override DateTime NextResetUtc(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        return _targetTimeUtc;
    }

    public override bool ShouldReset(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        
        // 이미 실행되었으면 더 이상 리셋하지 않음
        if (lastUtc >= _targetTimeUtc) return false;
        
        // 타겟 시간이 지났으면 리셋
        return Clock.UtcNow >= _targetTimeUtc - Latency;
    }

    public override TimeSpan TimeUntilReset(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        
        // 이미 실행되었으면 0 반환
        if (lastUtc >= _targetTimeUtc) return TimeSpan.Zero;
        
        var remaining = _targetTimeUtc - Clock.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// 설정된 실행 시점 (UTC)
    /// </summary>
    public DateTime TargetTimeUtc => _targetTimeUtc;

    /// <summary>
    /// 타이머가 이미 실행되었는지 확인
    /// </summary>
    /// <param name="lastUtc">마지막 확인 시간 (UTC)</param>
    /// <returns>이미 실행되었는지 여부</returns>
    public bool HasExecuted(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        return lastUtc >= _targetTimeUtc;
    }
}
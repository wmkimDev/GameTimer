using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 개별 인스턴스 기준 간격 타이머 (체력 충전, 쿨다운, 건설 시간 등)
/// 마지막 리셋 시점부터 정확히 interval 뒤에 다음 리셋
/// </summary>
public sealed class IntervalTimer : GlobalTimerBase
{
    private readonly TimeSpan _interval;

    /// <summary>
    /// 간격 타이머 생성
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="interval">간격</param>
    public IntervalTimer(IClock clock, TimeSpan interval) : base(clock)
    {
        if (interval <= TimeSpan.Zero)
            throw new ArgumentException("Interval must be positive", nameof(interval));
            
        _interval = interval;
    }
    
    /// <summary>
    /// 편의 생성자 - 시, 분, 초로 간격 설정
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="hours">시간</param>
    /// <param name="minutes">분</param>
    /// <param name="seconds">초</param>
    public IntervalTimer(IClock clock, int hours, int minutes = 0, int seconds = 0)
        : this(clock, new TimeSpan(hours, minutes, seconds))
    {
    }

    public override DateTime NextResetUtc(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        return lastUtc.Add(_interval);
    }

    /// <summary>
    /// 설정된 간격
    /// </summary>
    public TimeSpan Interval => _interval;
}
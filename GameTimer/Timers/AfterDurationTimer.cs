using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 시작 후 일정 시간 뒤 한 번만 완료 (건물 건설, 버프 지속시간, 아이템 제작 등)
/// 일회성 타이머로, 완료 후에는 더 이상 리셋되지 않음
/// 반복이 필요한 경우 IntervalTimer 사용 권장
/// </summary>
public sealed class AfterDurationTimer : GlobalTimerBase
{
    private readonly TimeSpan _duration;
    
    /// <summary>
    /// 일정 시간 후 한 번만 완료되는 타이머 생성
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="duration">지속 시간</param>
    public AfterDurationTimer(IClock clock, TimeSpan duration) : base(clock)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Duration must be positive", nameof(duration));
            
        _duration = duration;
    }

    /// <summary>
    /// 편의 생성자 - 시, 분, 초로 지속시간 설정
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="hours">시간</param>
    /// <param name="minutes">분</param>
    /// <param name="seconds">초</param>
    public AfterDurationTimer(IClock clock, int hours, int minutes = 0, int seconds = 0)
        : this(clock, new TimeSpan(hours, minutes, seconds))
    {
    }

    public override DateTime NextResetUtc(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        return lastUtc.Add(_duration);
    }
}
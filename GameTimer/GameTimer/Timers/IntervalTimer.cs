using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 개별 인스턴스 기준 간격 타이머 (체력 충전, 쿨다운, 건설 시간 등)
/// 마지막 리셋 시점부터 정확히 interval 뒤에 다음 리셋
/// </summary>
public sealed class IntervalTimer : IGlobalTimer
{
    private readonly IClock   _clock;
    private readonly TimeSpan _interval;
    public           TimeSpan Latency { get; set; } = TimeSpan.FromSeconds(2);

    public IntervalTimer(IClock clock, TimeSpan interval)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        if (interval <= TimeSpan.Zero)
            throw new ArgumentException("Interval must be positive", nameof(interval));
            
        _interval = interval;
    }

    public DateTime NextResetUtc(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        return lastUtc.Add(_interval);
    }

    public bool ShouldReset(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        var nextUtc = NextResetUtc(lastUtc);
        return _clock.UtcNow > nextUtc - Latency;
    }

    public TimeSpan TimeUntilReset(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        var nextUtc   = NextResetUtc(lastUtc);
        var remaining = nextUtc - _clock.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}
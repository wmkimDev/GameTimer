using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 글로벌 타이머 기본 클래스
/// </summary>
public abstract class GlobalTimerBase : IGlobalTimer
{
    protected readonly IClock   Clock;
    public             TimeSpan Latency { get; set; } = TimeSpan.FromSeconds(2);

    protected GlobalTimerBase(IClock clock)
    {
        Clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public abstract DateTime NextResetUtc(DateTime lastUtc);

    public virtual bool ShouldReset(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
            
        var nextUtc = NextResetUtc(lastUtc);
        return Clock.UtcNow > nextUtc - Latency;
    }

    public virtual TimeSpan TimeUntilReset(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
            
        var nextUtc   = NextResetUtc(lastUtc);
        var remaining = nextUtc - Clock.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}
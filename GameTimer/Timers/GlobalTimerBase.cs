using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 글로벌 타이머 기본 클래스
/// </summary>
public abstract class GlobalTimerBase : IGlobalTimer
{
    private          TimeSpan _latency = TimeSpan.FromSeconds(2);
    protected readonly IClock Clock;
    
    public TimeSpan Latency
    {
        get => _latency;
        set
        {
            if (value < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(value), "Latency must be non-negative");

            _latency = value;
        }
    }

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
    
    public DateTime NextUpcomingResetUtc() => NextResetUtc(Clock.UtcNow);
    public TimeSpan TimeUntilUpcomingReset() => TimeUntilReset(Clock.UtcNow);
}

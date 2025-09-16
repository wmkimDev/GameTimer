using GameTimer.Common;

namespace GameTimer.Timers;
using GameTimer.Abstractions;

// <summary>
/// 로컬 타이머 기본 클래스
/// </summary>
public abstract class LocalTimerBase : ILocalTimer
{
    protected readonly IClock   Clock;
    public             TimeSpan Latency { get; set; } = TimeSpan.FromSeconds(2);

    protected LocalTimerBase(IClock clock)
    {
        Clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public abstract DateTime NextResetUtc(DateTime lastUtc, TimeZoneInfo timeZone);

    public virtual bool ShouldReset(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));
            
        var nextUtc = NextResetUtc(lastUtc, timeZone);
        return Clock.UtcNow > nextUtc - Latency;
    }

    public virtual TimeSpan TimeUntilReset(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));
            
        var nextUtc   = NextResetUtc(lastUtc, timeZone);
        var remaining = nextUtc - Clock.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}
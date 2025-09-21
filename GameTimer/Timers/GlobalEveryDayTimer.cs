using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 글로벌: 매일 UTC 특정 시각에 동시 리셋 (예: 전 세계 동시 서버 이벤트)
/// </summary>
public sealed class GlobalEveryDayTimer : GlobalTimerBase
{
    private readonly TimeOfDay _resetTimeUtc;

    public GlobalEveryDayTimer(IClock clock, TimeOfDay resetTimeUtc) : base(clock)
    {
        _resetTimeUtc = resetTimeUtc;
    }

    public GlobalEveryDayTimer(IClock clock, int hour, int minute = 0, int second = 0) 
        : this(clock, new TimeOfDay(hour, minute, second))
    {
    }

    public override DateTime NextResetUtc(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));

        var sameDayReset = new DateTime(
            lastUtc.Year, lastUtc.Month, lastUtc.Day,
            _resetTimeUtc.Hour, _resetTimeUtc.Minute, _resetTimeUtc.Second,
            DateTimeKind.Utc);

        if (sameDayReset <= lastUtc)
            return sameDayReset.AddDays(1);

        return sameDayReset;
    }

    /// <summary>
    /// 설정된 UTC 리셋 시각
    /// </summary>
    public TimeOfDay ResetTimeUtc => _resetTimeUtc;
}
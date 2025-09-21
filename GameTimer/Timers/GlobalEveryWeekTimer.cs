using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 글로벌: 매주 UTC 기준 특정 요일과 시각에 동시 리셋
/// </summary>
public sealed class GlobalEveryWeekTimer : GlobalTimerBase
{
    private readonly DayOfWeekFlag _resetDays;
    private readonly TimeOfDay     _resetTimeUtc;

    public GlobalEveryWeekTimer(IClock clock, DayOfWeekFlag resetDays, TimeOfDay resetTimeUtc) : base(clock)
    {
        if (resetDays == 0)
            throw new ArgumentException("At least one day must be specified", nameof(resetDays));
            
        _resetDays    = resetDays;
        _resetTimeUtc = resetTimeUtc;
    }
    
    public GlobalEveryWeekTimer(IClock clock, DayOfWeekFlag resetDays, int hour, int minute = 0, int second = 0)
        : this(clock, resetDays, new TimeOfDay(hour, minute, second))
    {
    }

    public override DateTime NextResetUtc(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));

        var startDate = lastUtc.Date;

        for (int i = 0; i <= 7; i++)
        {
            var d = startDate.AddDays(i);
            if (!_resetDays.Contains(d.DayOfWeek))
                continue;

            var candidate = new DateTime(
                d.Year, d.Month, d.Day,
                _resetTimeUtc.Hour, _resetTimeUtc.Minute, _resetTimeUtc.Second,
                DateTimeKind.Utc);

            if (candidate > lastUtc)
                return candidate;
        }

        throw new InvalidOperationException("Could not find next weekly reset");
    }
}
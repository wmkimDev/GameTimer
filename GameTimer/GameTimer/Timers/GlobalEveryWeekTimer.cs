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

    public override DateTime NextResetUtc(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));

        for (int i = 0; i < 14; i++)
        {
            var candidate = lastUtc.Date.AddDays(i);
            var candidateReset = new DateTime(
                candidate.Year, candidate.Month, candidate.Day,
                _resetTimeUtc.Hour, _resetTimeUtc.Minute, _resetTimeUtc.Second,
                DateTimeKind.Utc);
                
            if (_resetDays.Contains(candidate.DayOfWeek) && lastUtc <= candidateReset)
            {
                return candidateReset;
            }
        }
            
        throw new InvalidOperationException("Could not find next weekly reset");
    }
}
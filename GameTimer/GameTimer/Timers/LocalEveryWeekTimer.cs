using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 로컬: 매주 현지시간 기준 특정 요일과 시각에 리셋
/// </summary>
public sealed class LocalEveryWeekTimer : LocalTimerBase
{
    private readonly DayOfWeekFlag _resetDays;
    private readonly TimeOfDay     _resetTime;

    public LocalEveryWeekTimer(IClock clock, DayOfWeekFlag resetDays, TimeOfDay resetTime) : base(clock)
    {
        if (resetDays == 0)
            throw new ArgumentException("At least one day must be specified", nameof(resetDays));
            
        _resetDays = resetDays;
        _resetTime = resetTime;
    }

    public override DateTime NextResetUtc(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));

        var lastLocal = TimeZoneInfo.ConvertTimeFromUtc(lastUtc, timeZone);
            
        for (int i = 0; i < 14; i++)
        {
            var candidate = lastLocal.Date.AddDays(i);
            var candidateReset = new DateTime(
                candidate.Year, candidate.Month, candidate.Day,
                _resetTime.Hour, _resetTime.Minute, _resetTime.Second,
                DateTimeKind.Unspecified);
                
            if (_resetDays.Contains(candidate.DayOfWeek) && lastLocal <= candidateReset)
            {
                return TimeZoneInfo.ConvertTimeToUtc(candidateReset, timeZone);
            }
        }
            
        throw new InvalidOperationException("Could not find next weekly reset");
    }
}
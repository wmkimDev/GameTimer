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

    public LocalEveryWeekTimer(IClock clock, DayOfWeekFlag resetDays, TimeOfDay resetTime, DstPolicy policy = DstPolicy.NextValid) : base(clock, policy)
    {
        if (resetDays == 0)
            throw new ArgumentException("At least one day must be specified", nameof(resetDays));
            
        _resetDays = resetDays;
        _resetTime = resetTime;
    }
    
    public LocalEveryWeekTimer(IClock clock, DayOfWeekFlag resetDays, int hour, int minute = 0, int second = 0, DstPolicy policy = DstPolicy.NextValid)
        : this(clock, resetDays, new TimeOfDay(hour, minute, second), policy)
    {
    }

    public override DateTime NextResetUtc(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));

        var lastLocal = TimeZoneInfo.ConvertTimeFromUtc(lastUtc, timeZone);
        var startDate = lastLocal.Date;

        for (int i = 0; i <= 7; i++)
        {
            var d = startDate.AddDays(i);
            if (!_resetDays.Contains(d.DayOfWeek))
                continue;

            var candidateLocal = new DateTime(
                d.Year, d.Month, d.Day,
                _resetTime.Hour, _resetTime.Minute, _resetTime.Second,
                DateTimeKind.Unspecified);

            if (candidateLocal > lastLocal)
                return DstHelpers.SafeConvertToUtc(candidateLocal, timeZone, Policy);
        }

        throw new InvalidOperationException("Could not find next weekly reset");
    }


    /// <summary>
    /// 설정된 리셋 요일들
    /// </summary>
    public DayOfWeekFlag ResetDays => _resetDays;

    /// <summary>
    /// 설정된 현지시간 리셋 시각
    /// </summary>
    public TimeOfDay ResetTime => _resetTime;
}

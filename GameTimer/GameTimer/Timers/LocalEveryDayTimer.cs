using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 로컬: 매일 현지시간 특정 시각에 리셋 (예: 각 지역별로 현지시간 오전 6시 일일퀘스트)
/// </summary>
public sealed class LocalEveryDayTimer : LocalTimerBase
{
    private readonly TimeOfDay _resetTime;

    public LocalEveryDayTimer(IClock clock, TimeOfDay resetTime) : base(clock)
    {
        _resetTime = resetTime;
    }

    public LocalEveryDayTimer(IClock clock, int hour, int minute = 0, int second = 0) 
        : this(clock, new TimeOfDay(hour, minute, second))
    {
    }

    public override DateTime NextResetUtc(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));

        var lastLocal = TimeZoneInfo.ConvertTimeFromUtc(lastUtc, timeZone);
            
        var todayReset = new DateTime(
            lastLocal.Year, lastLocal.Month, lastLocal.Day,
            _resetTime.Hour, _resetTime.Minute, _resetTime.Second, 
            DateTimeKind.Unspecified);
            
        var nextLocal = lastLocal <= todayReset ? todayReset : todayReset.AddDays(1);
        return TimeZoneInfo.ConvertTimeToUtc(nextLocal, timeZone);
    }
}
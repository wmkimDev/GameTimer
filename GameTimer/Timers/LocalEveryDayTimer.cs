using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 로컬: 매일 현지시간 특정 시각에 리셋 (예: 각 지역별로 현지시간 오전 6시 일일퀘스트)
/// </summary>
public sealed class LocalEveryDayTimer : LocalTimerBase
{
    private readonly TimeOfDay _resetTime;

    public LocalEveryDayTimer(IClock clock, TimeOfDay resetTime, DstPolicy policy = DstPolicy.NextValid) : base(clock, policy)
    {
        _resetTime = resetTime;
    }

    public LocalEveryDayTimer(IClock clock, int hour, int minute = 0, int second = 0, DstPolicy policy = DstPolicy.NextValid) 
        : this(clock, new TimeOfDay(hour, minute, second), policy)
    {
    }

    public override DateTime NextResetUtc(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));

        var lastLocal = TimeZoneInfo.ConvertTimeFromUtc(lastUtc, timeZone);
        var day       = lastLocal.Date;

        for (int i = 0; i <= 1; i++)
        {
            var d = day.AddDays(i);
            var candidateLocal = new DateTime(
                d.Year, d.Month, d.Day,
                _resetTime.Hour, _resetTime.Minute, _resetTime.Second,
                DateTimeKind.Unspecified);

            if (candidateLocal > lastLocal)
                return DstHelpers.SafeConvertToUtc(candidateLocal, timeZone, Policy);
        }

        throw new InvalidOperationException("Could not find next daily reset");
    }

    /// <summary>
    /// 설정된 현지시간 리셋 시각
    /// </summary>
    public TimeOfDay ResetTime => _resetTime;
}

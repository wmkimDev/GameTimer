using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 로컬: 매일 여러 현지시간에 리셋 (예: 매일 00:00, 06:00, 12:00, 18:00)
/// </summary>
public sealed class LocalMultipleTimesTimer : LocalTimerBase
{
    private readonly TimeOfDay[] _resetTimes;

    public LocalMultipleTimesTimer(IClock clock, params TimeOfDay[] resetTimes)
        : this(clock, DstPolicy.NextValid, resetTimes)
    {
    }

    // 정책 지정 가능한 오버로드 (params 특성상 policy를 앞에 둠)
    public LocalMultipleTimesTimer(IClock clock, DstPolicy policy, params TimeOfDay[] resetTimes) : base(clock, policy)
    {
        _resetTimes = PrepareResetTimes(resetTimes, nameof(resetTimes));
    }

    /// <summary>
    /// 편의 생성자 - 시간들을 개별 매개변수로 설정
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="hours">시간 배열 (0-23)</param>
    public LocalMultipleTimesTimer(IClock clock, params int[] hours) 
        : this(clock, DstPolicy.NextValid, hours)
    {
    }

    public LocalMultipleTimesTimer(IClock clock, DstPolicy policy, params int[] hours)
        : base(clock, policy)
    {
        var timeOfDays = ConvertHoursToTimeOfDay(hours, nameof(hours));
        _resetTimes = PrepareResetTimes(timeOfDays, nameof(hours));
    }

    public override DateTime NextResetUtc(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));

        var lastLocal = TimeZoneInfo.ConvertTimeFromUtc(lastUtc, timeZone);
        var day       = lastLocal.Date;

        foreach (var t in _resetTimes)
        {
            var candidateLocal = new DateTime(
                day.Year, day.Month, day.Day,
                t.Hour, t.Minute, t.Second,
                DateTimeKind.Unspecified);

            if (candidateLocal > lastLocal)
                return DstHelpers.SafeConvertToUtc(candidateLocal, timeZone, Policy);
        }

        var nextDay = day.AddDays(1);
        var first   = _resetTimes[0];
        var nextLocal = new DateTime(
            nextDay.Year, nextDay.Month, nextDay.Day,
            first.Hour, first.Minute, first.Second,
            DateTimeKind.Unspecified);

        return DstHelpers.SafeConvertToUtc(nextLocal, timeZone, Policy);
    }

    /// <summary>
    /// 설정된 리셋 시각들
    /// </summary>
    public IReadOnlyList<TimeOfDay> ResetTimes => _resetTimes;

    private static TimeOfDay[] PrepareResetTimes(TimeOfDay[] resetTimes, string parameterName)
    {
        if (resetTimes == null)
            throw new ArgumentNullException(parameterName);

        if (resetTimes.Length == 0)
            throw new ArgumentException("At least one reset time must be specified.", parameterName);

        var ordered = resetTimes.OrderBy(t => t).ToArray();

        for (var i = 1; i < ordered.Length; i++)
        {
            if (ordered[i] == ordered[i - 1])
                throw new ArgumentException("Reset times must be unique.", parameterName);
        }

        return ordered;
    }

    private static TimeOfDay[] ConvertHoursToTimeOfDay(int[] hours, string parameterName)
    {
        if (hours == null)
            throw new ArgumentNullException(parameterName);

        return hours.Select(h => new TimeOfDay(h, 0, 0)).ToArray();
    }
}

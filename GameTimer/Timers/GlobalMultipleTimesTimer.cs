using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 글로벌: 매일 여러 UTC 시각에 동시 리셋 (예: 매일 00:00, 08:00, 16:00 UTC)
/// </summary>
public sealed class GlobalMultipleTimesTimer : GlobalTimerBase
{
    private readonly TimeOfDay[] _resetTimesUtc;

    /// <summary>
    /// 여러 UTC 시각에 리셋되는 글로벌 타이머 생성
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="resetTimesUtc">리셋될 UTC 시각들</param>
    public GlobalMultipleTimesTimer(IClock clock, params TimeOfDay[] resetTimesUtc) : base(clock)
    {
        if (resetTimesUtc == null || resetTimesUtc.Length == 0)
            throw new ArgumentException("At least one reset time must be specified", nameof(resetTimesUtc));

        _resetTimesUtc = resetTimesUtc.OrderBy(t => t).ToArray();
    }

    /// <summary>
    /// 편의 생성자 - 시간들을 개별 매개변수로 설정
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="hours">UTC 시간 배열 (0-23)</param>
    public GlobalMultipleTimesTimer(IClock clock, params int[] hours) 
        : this(clock, hours.Select(h => new TimeOfDay(h, 0, 0)).ToArray())
    {
    }

    public override DateTime NextResetUtc(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));

        var day = lastUtc.Date;

        foreach (var t in _resetTimesUtc)
        {
            var candidate = new DateTime(
                day.Year, day.Month, day.Day,
                t.Hour, t.Minute, t.Second,
                DateTimeKind.Utc);

            if (candidate > lastUtc)
                return candidate;
        }

        var nextDay = day.AddDays(1);
        var first   = _resetTimesUtc[0];
        return new DateTime(
            nextDay.Year, nextDay.Month, nextDay.Day,
            first.Hour, first.Minute, first.Second,
            DateTimeKind.Utc);
    }

    /// <summary>
    /// 설정된 UTC 리셋 시각들
    /// </summary>
    public IReadOnlyList<TimeOfDay> ResetTimesUtc => _resetTimesUtc;
}
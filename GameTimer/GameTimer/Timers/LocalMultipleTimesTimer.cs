using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 로컬: 매일 여러 현지시간에 리셋 (예: 매일 00:00, 06:00, 12:00, 18:00)
/// </summary>
public sealed class LocalMultipleTimesTimer : ILocalTimer
{
    private readonly IClock      _clock;
    private readonly TimeOfDay[] _resetTimes;
    public           TimeSpan    Latency { get; set; } = TimeSpan.FromSeconds(2);

    public LocalMultipleTimesTimer(IClock clock, params TimeOfDay[] resetTimes)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        if (resetTimes == null || resetTimes.Length == 0)
            throw new ArgumentException("At least one reset time must be specified", nameof(resetTimes));

        _resetTimes = resetTimes.OrderBy(t => t).ToArray();
    }

    public DateTime NextResetUtc(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));

        var lastLocal = TimeZoneInfo.ConvertTimeFromUtc(lastUtc, timeZone);

        // 오늘 남은 시각들 중 가장 빠른 것 찾기
        foreach (var resetTime in _resetTimes)
        {
            var todayReset = new DateTime(
                lastLocal.Year, lastLocal.Month, lastLocal.Day,
                resetTime.Hour, resetTime.Minute, resetTime.Second,
                DateTimeKind.Unspecified);

            if (lastLocal <= todayReset)
            {
                return TimeZoneInfo.ConvertTimeToUtc(todayReset, timeZone);
            }
        }

        // 오늘 모든 시각이 지났으면 내일 첫 번째 시각
        var tomorrowFirst = new DateTime(
            lastLocal.Year, lastLocal.Month, lastLocal.Day + 1,
            _resetTimes[0].Hour, _resetTimes[0].Minute, _resetTimes[0].Second,
            DateTimeKind.Unspecified);

        return TimeZoneInfo.ConvertTimeToUtc(tomorrowFirst, timeZone);
    }

    public bool ShouldReset(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));

        var nextUtc = NextResetUtc(lastUtc, timeZone);
        return _clock.UtcNow > nextUtc - Latency;
    }

    public TimeSpan TimeUntilReset(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));

        var nextUtc   = NextResetUtc(lastUtc, timeZone);
        var remaining = nextUtc - _clock.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}
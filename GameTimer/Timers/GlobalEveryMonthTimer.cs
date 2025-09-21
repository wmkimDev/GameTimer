using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 글로벌: 매월 UTC 기준 특정 날짜와 시각에 동시 리셋
/// </summary>
public sealed class GlobalEveryMonthTimer : GlobalTimerBase
{
    /// <summary>
    /// 월의 마지막 날을 나타내는 상수
    /// </summary>
    public const int LastDay = -1;
    
    private readonly int       _dayOfMonth;
    private readonly TimeOfDay _resetTimeUtc;

    /// <summary>
    /// 매월 특정 날짜와 UTC 시각에 리셋되는 글로벌 타이머 생성
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="dayOfMonth">월의 날짜 (1-31 또는 LastDay)</param>
    /// <param name="resetTimeUtc">리셋될 UTC 시각</param>
    public GlobalEveryMonthTimer(IClock clock, int dayOfMonth, TimeOfDay resetTimeUtc) : base(clock)
    {
        if (dayOfMonth == 0 || (dayOfMonth < LastDay) || dayOfMonth > 31)
            throw new ArgumentOutOfRangeException(nameof(dayOfMonth), 
                "Day of month must be 1-31 or LastDay");
            
        _dayOfMonth = dayOfMonth;
        _resetTimeUtc = resetTimeUtc;
    }

    /// <summary>
    /// 편의 생성자 - 날짜와 시각을 개별 매개변수로 설정
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="dayOfMonth">월의 날짜 (1-31 또는 LastDay)</param>
    /// <param name="hour">UTC 시간 (0-23)</param>
    /// <param name="minute">UTC 분 (0-59)</param>
    /// <param name="second">UTC 초 (0-59)</param>
    public GlobalEveryMonthTimer(IClock clock, int dayOfMonth, int hour, int minute = 0, int second = 0)
        : this(clock, dayOfMonth, new TimeOfDay(hour, minute, second))
    {
    }

    /// <summary>
    /// 매월 말일에 리셋되는 타이머 생성 (정적 팩토리 메서드)
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="resetTimeUtc">리셋될 UTC 시각</param>
    /// <returns>월말일 타이머</returns>
    public static GlobalEveryMonthTimer LastDayOfMonth(IClock clock, TimeOfDay resetTimeUtc)
    {
        return new GlobalEveryMonthTimer(clock, LastDay, resetTimeUtc);
    }

    /// <summary>
    /// 매월 말일에 리셋되는 타이머 생성 (편의 메서드)
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="hour">UTC 시간 (0-23)</param>
    /// <param name="minute">UTC 분 (0-59)</param>
    /// <param name="second">UTC 초 (0-59)</param>
    /// <returns>월말일 타이머</returns>
    public static GlobalEveryMonthTimer LastDayOfMonth(IClock clock, int hour, int minute = 0, int second = 0)
    {
        return new GlobalEveryMonthTimer(clock, LastDay, hour, minute, second);
    }

    public override DateTime NextResetUtc(DateTime lastUtc)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));

        var monthCursor = new DateTime(lastUtc.Year, lastUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        for (int i = 0; i < 13; i++)
        {
            var candidate = CreateMonthlyResetTime(monthCursor);

            if (candidate > lastUtc)
                return candidate;

            monthCursor = monthCursor.AddMonths(1);
        }

        throw new InvalidOperationException("Could not find next monthly reset");
    }


    /// <summary>
    /// 지정된 월에서 리셋 시간을 생성
    /// </summary>
    private DateTime CreateMonthlyResetTime(DateTime monthStart)
    {
        int targetDay;
        
        if (_dayOfMonth == LastDay)
        {
            // 월말일 처리
            targetDay = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
        }
        else
        {
            // 지정된 날짜, 해당 월에 존재하지 않으면 월말일로 조정
            var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
            targetDay = Math.Min(_dayOfMonth, daysInMonth);
        }

        return new DateTime(
            monthStart.Year, monthStart.Month, targetDay,
            _resetTimeUtc.Hour, _resetTimeUtc.Minute, _resetTimeUtc.Second,
            DateTimeKind.Utc);
    }

    /// <summary>
    /// 설정된 월의 날짜
    /// </summary>
    public int DayOfMonth => _dayOfMonth;

    /// <summary>
    /// 설정된 UTC 리셋 시각
    /// </summary>
    public TimeOfDay ResetTimeUtc => _resetTimeUtc;
}
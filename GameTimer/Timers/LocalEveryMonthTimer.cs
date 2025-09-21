using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

/// <summary>
/// 로컬: 매월 현지시간 기준 특정 날짜와 시각에 리셋
/// </summary>
public sealed class LocalEveryMonthTimer : LocalTimerBase
{
    /// <summary>
    /// 월의 마지막 날을 나타내는 상수
    /// </summary>
    public const int LastDay = -1;
    
    private readonly int       _dayOfMonth;
    private readonly TimeOfDay _resetTime;

    /// <summary>
    /// 매월 특정 날짜와 현지시간 시각에 리셋되는 로컬 타이머 생성
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="dayOfMonth">월의 날짜 (1-31 또는 LastDay)</param>
    /// <param name="resetTime">리셋될 현지시간 시각</param>
    public LocalEveryMonthTimer(IClock clock, int dayOfMonth, TimeOfDay resetTime, DstPolicy policy = DstPolicy.NextValid) : base(clock, policy)
    {
        if (dayOfMonth == 0 || (dayOfMonth < LastDay) || dayOfMonth > 31)
            throw new ArgumentOutOfRangeException(nameof(dayOfMonth), 
                "Day of month must be 1-31 or LastDay");
            
        _dayOfMonth = dayOfMonth;
        _resetTime = resetTime;
    }

    /// <summary>
    /// 편의 생성자 - 날짜와 시각을 개별 매개변수로 설정
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="dayOfMonth">월의 날짜 (1-31 또는 LastDay)</param>
    /// <param name="hour">현지시간 시간 (0-23)</param>
    /// <param name="minute">현지시간 분 (0-59)</param>
    /// <param name="second">현지시간 초 (0-59)</param>
    public LocalEveryMonthTimer(IClock clock, int dayOfMonth, int hour, int minute = 0, int second = 0, DstPolicy policy = DstPolicy.NextValid)
        : this(clock, dayOfMonth, new TimeOfDay(hour, minute, second), policy)
    {
    }

    /// <summary>
    /// 매월 말일에 리셋되는 타이머 생성 (정적 팩토리 메서드)
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="resetTime">리셋될 현지시간 시각</param>
    /// <returns>월말일 타이머</returns>
    public static LocalEveryMonthTimer LastDayOfMonth(IClock clock, TimeOfDay resetTime, DstPolicy policy = DstPolicy.NextValid)
    {
        return new LocalEveryMonthTimer(clock, LastDay, resetTime, policy);
    }

    /// <summary>
    /// 매월 말일에 리셋되는 타이머 생성 (편의 메서드)
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="hour">현지시간 시간 (0-23)</param>
    /// <param name="minute">현지시간 분 (0-59)</param>
    /// <param name="second">현지시간 초 (0-59)</param>
    /// <returns>월말일 타이머</returns>
    public static LocalEveryMonthTimer LastDayOfMonth(IClock clock, int hour, int minute = 0, int second = 0, DstPolicy policy = DstPolicy.NextValid)
    {
        return new LocalEveryMonthTimer(clock, LastDay, hour, minute, second, policy);
    }

    public override DateTime NextResetUtc(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));

        var lastLocal = TimeZoneInfo.ConvertTimeFromUtc(lastUtc, timeZone);

        var monthCursorLocal = new DateTime(
            lastLocal.Year, lastLocal.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);

        for (int i = 0; i < 13; i++)
        {
            var candidateUtc   = CreateMonthlyResetTime(monthCursorLocal, timeZone);
            var candidateLocal = TimeZoneInfo.ConvertTimeFromUtc(candidateUtc, timeZone);

            if (candidateLocal > lastLocal) 
                return candidateUtc;

            monthCursorLocal = monthCursorLocal.AddMonths(1);
        }

        throw new InvalidOperationException("Could not find next monthly reset");
    }


    /// <summary>
    /// 지정된 월에서 리셋 시간을 생성 (UTC로 반환)
    /// </summary>
    private DateTime CreateMonthlyResetTime(DateTime monthStart, TimeZoneInfo timeZone)
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

        var localReset = new DateTime(
            monthStart.Year, monthStart.Month, targetDay,
            _resetTime.Hour, _resetTime.Minute, _resetTime.Second,
            DateTimeKind.Unspecified);

        // DST 인식 안전한 변환 사용
        return DstHelpers.SafeConvertToUtc(localReset, timeZone, Policy);
    }

    /// <summary>
    /// 설정된 월의 날짜
    /// </summary>
    public int DayOfMonth => _dayOfMonth;

    /// <summary>
    /// 설정된 현지시간 리셋 시각
    /// </summary>
    public TimeOfDay ResetTime => _resetTime;
}

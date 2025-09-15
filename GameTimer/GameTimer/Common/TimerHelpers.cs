namespace GameTimer.Common;

public static class TimerHelpers
{
    /// <summary>
    /// DateTime이 UTC인지 검증하고, 아니면 예외 발생
    /// </summary>
    /// <param name="dateTime">검증할 DateTime</param>
    /// <param name="paramName">매개변수 이름</param>
    /// <exception cref="ArgumentException">UTC가 아닌 경우</exception>
    public static void ValidateUtc(DateTime dateTime, string paramName = "dateTime")
    {
        if (dateTime.Kind != DateTimeKind.Utc)
            throw new ArgumentException($"{paramName} must be UTC", paramName);
    }
    
    public static DayOfWeekFlag ToFlag(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Sunday    => DayOfWeekFlag.Sunday,
            DayOfWeek.Monday    => DayOfWeekFlag.Monday,
            DayOfWeek.Tuesday   => DayOfWeekFlag.Tuesday,
            DayOfWeek.Wednesday => DayOfWeekFlag.Wednesday,
            DayOfWeek.Thursday  => DayOfWeekFlag.Thursday,
            DayOfWeek.Friday    => DayOfWeekFlag.Friday,
            DayOfWeek.Saturday  => DayOfWeekFlag.Saturday,
            _                   => throw new ArgumentOutOfRangeException(nameof(dayOfWeek))
        };
    }
    
    /// <summary>
    /// DayOfWeekFlag가 특정 System.DayOfWeek를 포함하는지 확인
    /// </summary>
    /// <param name="flags">DayOfWeekFlag</param>
    /// <param name="dayOfWeek">확인할 System.DayOfWeek</param>
    /// <returns>포함 여부</returns>
    public static bool Contains(this DayOfWeekFlag flags, DayOfWeek dayOfWeek)
    {
        return flags.HasFlag(dayOfWeek.ToFlag());
    }
    
    /// <summary>
    /// 월의 마지막 날짜 계산 (윤년 고려)
    /// </summary>
    /// <param name="year">년</param>
    /// <param name="month">월</param>
    /// <returns>해당 월의 마지막 날</returns>
    public static int GetLastDayOfMonth(int year, int month)
    {
        return DateTime.DaysInMonth(year, month);
    }
    
    /// <summary>
    /// 지정된 날짜가 월의 마지막 날인지 확인
    /// </summary>
    /// <param name="date">확인할 날짜</param>
    /// <returns>마지막 날 여부</returns>
    public static bool IsLastDayOfMonth(DateTime date)
    {
        return date.Day == GetLastDayOfMonth(date.Year, date.Month);
    }
}
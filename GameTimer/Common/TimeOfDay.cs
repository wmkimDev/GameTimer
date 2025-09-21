namespace GameTimer.Common;

public readonly struct TimeOfDay : IEquatable<TimeOfDay>, IComparable<TimeOfDay>
{
    /// <summary>
    /// 시 (0-23)
    /// </summary>
    public int Hour { get; }

    /// <summary>
    /// 분 (0-59)
    /// </summary>
    public int Minute { get; }

    /// <summary>
    /// 초 (0-59)
    /// </summary>
    public int Second { get; }

    /// <summary>
    /// 밀리초 (0-999)
    /// </summary>
    public int Millisecond { get; }
    
    public TimeOfDay(int hour, int minute = 0, int second = 0, int millisecond = 0)
    {
        if (hour < 0 || hour > 23)
            throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 23");
        if (minute < 0 || minute > 59)
            throw new ArgumentOutOfRangeException(nameof(minute), "Minute must be between 0 and 59");
        if (second < 0 || second > 59)
            throw new ArgumentOutOfRangeException(nameof(second), "Second must be between 0 and 59");
        if (millisecond < 0 || millisecond > 999)
            throw new ArgumentOutOfRangeException(nameof(millisecond), "Millisecond must be between 0 and 999");

        Hour        = hour;
        Minute      = minute;
        Second      = second;
        Millisecond = millisecond;
    }
    
    public TimeOfDay(DateTime dateTime)
    {
        Hour        = dateTime.Hour;
        Minute      = dateTime.Minute;
        Second      = dateTime.Second;
        Millisecond = dateTime.Millisecond;
    }
    
    public TimeOfDay(TimeSpan timeSpan)
    {
        long ticks           = timeSpan.Ticks % TimeSpan.TicksPerDay;
        if (ticks < 0) ticks += TimeSpan.TicksPerDay;

        Hour  =  (int)(ticks / TimeSpan.TicksPerHour);
        ticks %= TimeSpan.TicksPerHour;

        Minute =  (int)(ticks / TimeSpan.TicksPerMinute);
        ticks  %= TimeSpan.TicksPerMinute;

        Second      = (int)(ticks / TimeSpan.TicksPerSecond);
        Millisecond = (int)((ticks % TimeSpan.TicksPerSecond) / TimeSpan.TicksPerMillisecond);
    }
    
    public TimeSpan ToTimeSpan()
    {
        return new TimeSpan(0, Hour, Minute, Second, Millisecond);
    }
    
    public override string ToString()
    {
        return $"{Hour:00}:{Minute:00}:{Second:00}.{Millisecond:000}";
    }
    
    #region Equality and Comparison
    
    public bool Equals(TimeOfDay other)
    {
        return Hour == other.Hour && Minute == other.Minute && 
               Second == other.Second && Millisecond == other.Millisecond;
    }
    public override bool Equals(object? obj)
    {
        return obj is TimeOfDay other && Equals(other);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Hour, Minute, Second, Millisecond);
    }
    
    public int CompareTo(TimeOfDay other)
    {
        if (Hour != other.Hour) return Hour.CompareTo(other.Hour);
        if (Minute != other.Minute) return Minute.CompareTo(other.Minute);
        if (Second != other.Second) return Second.CompareTo(other.Second);
        return Millisecond.CompareTo(other.Millisecond);
    }
    
    public static bool operator ==(TimeOfDay left, TimeOfDay right) => left.Equals(right);
    public static bool operator !=(TimeOfDay left, TimeOfDay right) => !left.Equals(right);
    public static bool operator <(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) < 0;
    public static bool operator <=(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) <= 0;
    public static bool operator >(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) > 0;
    public static bool operator >=(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) >= 0;

    #endregion
    
    public TimeOfDay Set(int? hour = null, int? minute = null, 
        int? second = null, int? millisecond = null)
    {
        return new TimeOfDay(
            hour ?? Hour,
            minute ?? Minute, 
            second ?? Second,
            millisecond ?? Millisecond
        );
    }
}

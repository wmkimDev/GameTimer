using GameTimer.Abstractions;
using GameTimer.Common;
using GameTimer.Timers;

namespace GameTimer.Builder;

public static class GlobalTimers
{
    public static GlobalDailyBuilder Daily(IClock clock) => new(clock);
    public static GlobalWeeklyBuilder Weekly(IClock clock) => new(clock);
    public static GlobalMonthlyBuilder Monthly(IClock clock) => new(clock);
    public static GlobalMultipleTimesBuilder MultipleTimes(IClock clock) => new(clock);

    // Direct factories for one-shot and interval-based timers
    public static GlobalOnceAtTimer OnceAt(IClock clock, DateTime targetUtc) => new(clock, targetUtc);
    public static GlobalOnceAtTimer OnceAt(IClock clock, int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
        => new(clock, year, month, day, hour, minute, second);

    public static IntervalTimer Every(IClock clock, TimeSpan interval) => new(clock, interval);
    public static IntervalTimer Every(IClock clock, int hours, int minutes = 0, int seconds = 0) => new(clock, hours, minutes, seconds);

    public static AfterDurationTimer After(IClock clock, TimeSpan duration) => new(clock, duration);
    public static AfterDurationTimer After(IClock clock, int hours, int minutes = 0, int seconds = 0) => new(clock, hours, minutes, seconds);
}

public sealed class GlobalDailyBuilder
{
    private readonly IClock _clock;
    private TimeOfDay _time = new(0, 0, 0);
    private TimeSpan? _latency;

    public GlobalDailyBuilder(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public GlobalDailyBuilder UtcAt(int hour, int minute = 0, int second = 0)
    {
        _time = new TimeOfDay(hour, minute, second);
        return this;
    }

    public GlobalDailyBuilder UtcAt(TimeOfDay time)
    {
        _time = time;
        return this;
    }

    public GlobalDailyBuilder WithLatency(TimeSpan latency)
    {
        if (latency < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(latency), "Latency must be non-negative.");

        _latency = latency;
        return this;
    }

    public GlobalEveryDayTimer Build()
    {
        var t = new GlobalEveryDayTimer(_clock, _time);
        if (_latency.HasValue) t.Latency = _latency.Value;
        return t;
    }
}

public sealed class GlobalWeeklyBuilder
{
    private readonly IClock _clock;
    private DayOfWeekFlag _days = DayOfWeekFlag.None;
    private TimeOfDay _time = new(0, 0, 0);
    private TimeSpan? _latency;

    public GlobalWeeklyBuilder(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public GlobalWeeklyBuilder On(DayOfWeekFlag days)
    {
        _days = days;
        return this;
    }

    public GlobalWeeklyBuilder UtcAt(int hour, int minute = 0, int second = 0)
    {
        _time = new TimeOfDay(hour, minute, second);
        return this;
    }

    public GlobalWeeklyBuilder UtcAt(TimeOfDay time)
    {
        _time = time;
        return this;
    }

    public GlobalWeeklyBuilder WithLatency(TimeSpan latency)
    {
        if (latency < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(latency), "Latency must be non-negative.");

        _latency = latency;
        return this;
    }

    public GlobalEveryWeekTimer Build()
    {
        var t = new GlobalEveryWeekTimer(_clock, _days, _time);
        if (_latency.HasValue) t.Latency = _latency.Value;
        return t;
    }
}

public sealed class GlobalMonthlyBuilder
{
    private readonly IClock _clock;
    private int _dayOfMonth = 1;
    private TimeOfDay _time = new(0, 0, 0);
    private TimeSpan? _latency;

    public GlobalMonthlyBuilder(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public GlobalMonthlyBuilder On(int dayOfMonth)
    {
        _dayOfMonth = dayOfMonth;
        return this;
    }

    public GlobalMonthlyBuilder LastDay()
    {
        _dayOfMonth = GlobalEveryMonthTimer.LastDay;
        return this;
    }

    public GlobalMonthlyBuilder UtcAt(int hour, int minute = 0, int second = 0)
    {
        _time = new TimeOfDay(hour, minute, second);
        return this;
    }

    public GlobalMonthlyBuilder UtcAt(TimeOfDay time)
    {
        _time = time;
        return this;
    }

    public GlobalMonthlyBuilder WithLatency(TimeSpan latency)
    {
        if (latency < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(latency), "Latency must be non-negative.");

        _latency = latency;
        return this;
    }

    public GlobalEveryMonthTimer Build()
    {
        var t = new GlobalEveryMonthTimer(_clock, _dayOfMonth, _time);
        if (_latency.HasValue) t.Latency = _latency.Value;
        return t;
    }
}

public sealed class GlobalMultipleTimesBuilder
{
    private readonly IClock _clock;
    private TimeOfDay[]? _times;
    private TimeSpan? _latency;

    public GlobalMultipleTimesBuilder(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public GlobalMultipleTimesBuilder UtcAt(params TimeOfDay[] times)
    {
        _times = times;
        return this;
    }

    public GlobalMultipleTimesBuilder UtcAtHours(params int[] hours)
    {
        _times = hours.Select(h => new TimeOfDay(h, 0, 0)).ToArray();
        return this;
    }

    public GlobalMultipleTimesBuilder WithLatency(TimeSpan latency)
    {
        if (latency < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(latency), "Latency must be non-negative.");

        _latency = latency;
        return this;
    }

    public GlobalMultipleTimesTimer Build()
    {
        if (_times == null || _times.Length == 0)
            throw new InvalidOperationException("At least one reset time must be provided.");

        var t = new GlobalMultipleTimesTimer(_clock, _times);
        if (_latency.HasValue) t.Latency = _latency.Value;
        return t;
    }
}

using GameTimer.Abstractions;
using GameTimer.Common;
using GameTimer.Timers;

namespace GameTimer.Builder;

public static class LocalTimers
{
    public static LocalDailyBuilder Daily(IClock clock, DstPolicy policy = DstPolicy.NextValid) => new(clock, policy);
    public static LocalWeeklyBuilder Weekly(IClock clock, DstPolicy policy = DstPolicy.NextValid) => new(clock, policy);
    public static LocalMonthlyBuilder Monthly(IClock clock, DstPolicy policy = DstPolicy.NextValid) => new(clock, policy);
    public static LocalMultipleTimesBuilder MultipleTimes(IClock clock, DstPolicy policy = DstPolicy.NextValid) => new(clock, policy);

    // Direct factories where builder adds little value
    public static LocalOnceAtTimer OnceAt(IClock clock, DateTime targetLocal, DstPolicy policy = DstPolicy.NextValid)
        => new(clock, targetLocal, policy);

    public static LocalOnceAtTimer OnceAt(IClock clock, int year, int month, int day,
        int hour = 0, int minute = 0, int second = 0, DstPolicy policy = DstPolicy.NextValid)
        => new(clock, year, month, day, hour, minute, second, policy);
}

public sealed class LocalDailyBuilder
{
    private readonly IClock _clock;
    private DstPolicy _policy;
    private TimeOfDay _time = new(0, 0, 0);
    private TimeSpan? _latency;

    public LocalDailyBuilder(IClock clock, DstPolicy policy = DstPolicy.NextValid)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _policy = policy;
    }

    public LocalDailyBuilder WithPolicy(DstPolicy policy)
    {
        _policy = policy;
        return this;
    }

    public LocalDailyBuilder At(int hour, int minute = 0, int second = 0)
    {
        _time = new TimeOfDay(hour, minute, second);
        return this;
    }

    public LocalDailyBuilder At(TimeOfDay time)
    {
        _time = time;
        return this;
    }

    public LocalDailyBuilder WithLatency(TimeSpan latency)
    {
        if (latency < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(latency), "Latency must be non-negative.");

        _latency = latency;
        return this;
    }

    public LocalEveryDayTimer Build()
    {
        var t = new LocalEveryDayTimer(_clock, _time, _policy);
        if (_latency.HasValue) t.Latency = _latency.Value;
        return t;
    }
}

public sealed class LocalWeeklyBuilder
{
    private readonly IClock _clock;
    private DstPolicy _policy;
    private DayOfWeekFlag _days = DayOfWeekFlag.None;
    private TimeOfDay _time = new(0, 0, 0);
    private TimeSpan? _latency;

    public LocalWeeklyBuilder(IClock clock, DstPolicy policy = DstPolicy.NextValid)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _policy = policy;
    }

    public LocalWeeklyBuilder WithPolicy(DstPolicy policy)
    {
        _policy = policy;
        return this;
    }

    public LocalWeeklyBuilder On(DayOfWeekFlag days)
    {
        _days = days;
        return this;
    }

    public LocalWeeklyBuilder At(int hour, int minute = 0, int second = 0)
    {
        _time = new TimeOfDay(hour, minute, second);
        return this;
    }

    public LocalWeeklyBuilder At(TimeOfDay time)
    {
        _time = time;
        return this;
    }

    public LocalWeeklyBuilder WithLatency(TimeSpan latency)
    {
        if (latency < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(latency), "Latency must be non-negative.");

        _latency = latency;
        return this;
    }

    public LocalEveryWeekTimer Build()
    {
        var t = new LocalEveryWeekTimer(_clock, _days, _time, _policy);
        if (_latency.HasValue) t.Latency = _latency.Value;
        return t;
    }
}

public sealed class LocalMonthlyBuilder
{
    private readonly IClock _clock;
    private DstPolicy _policy;
    private int _dayOfMonth = 1;
    private TimeOfDay _time = new(0, 0, 0);
    private TimeSpan? _latency;

    public LocalMonthlyBuilder(IClock clock, DstPolicy policy = DstPolicy.NextValid)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _policy = policy;
    }

    public LocalMonthlyBuilder WithPolicy(DstPolicy policy)
    {
        _policy = policy;
        return this;
    }

    public LocalMonthlyBuilder On(int dayOfMonth)
    {
        _dayOfMonth = dayOfMonth;
        return this;
    }

    public LocalMonthlyBuilder LastDay()
    {
        _dayOfMonth = LocalEveryMonthTimer.LastDay;
        return this;
    }

    public LocalMonthlyBuilder At(int hour, int minute = 0, int second = 0)
    {
        _time = new TimeOfDay(hour, minute, second);
        return this;
    }

    public LocalMonthlyBuilder At(TimeOfDay time)
    {
        _time = time;
        return this;
    }

    public LocalMonthlyBuilder WithLatency(TimeSpan latency)
    {
        if (latency < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(latency), "Latency must be non-negative.");

        _latency = latency;
        return this;
    }

    public LocalEveryMonthTimer Build()
    {
        var t = new LocalEveryMonthTimer(_clock, _dayOfMonth, _time, _policy);
        if (_latency.HasValue) t.Latency = _latency.Value;
        return t;
    }
}

public sealed class LocalMultipleTimesBuilder
{
    private readonly IClock _clock;
    private DstPolicy _policy;
    private TimeOfDay[]? _times;
    private TimeSpan? _latency;

    public LocalMultipleTimesBuilder(IClock clock, DstPolicy policy = DstPolicy.NextValid)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _policy = policy;
    }

    public LocalMultipleTimesBuilder WithPolicy(DstPolicy policy)
    {
        _policy = policy;
        return this;
    }

    public LocalMultipleTimesBuilder At(params TimeOfDay[] times)
    {
        _times = times;
        return this;
    }

    public LocalMultipleTimesBuilder AtHours(params int[] hours)
    {
        _times = hours.Select(h => new TimeOfDay(h, 0, 0)).ToArray();
        return this;
    }

    public LocalMultipleTimesBuilder WithLatency(TimeSpan latency)
    {
        if (latency < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(latency), "Latency must be non-negative.");

        _latency = latency;
        return this;
    }
    
    public LocalMultipleTimesTimer Build()
    {
        if (_times == null || _times.Length == 0)
            throw new InvalidOperationException("At least one reset time must be provided.");

        // Use the policy-aware ctor overload to avoid params ambiguity
        var t                            = new LocalMultipleTimesTimer(_clock, _policy, _times);
        if (_latency.HasValue) t.Latency = _latency.Value;
        return t;
    }
}

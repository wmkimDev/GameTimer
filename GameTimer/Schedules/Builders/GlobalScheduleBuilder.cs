namespace GameTimer.Schedules.Builders;

using System;
using System.Collections.Generic;
using GameTimer.Abstractions;
using Common;
using Abstractions;
using Models;
using Schedules;

public static class GlobalSchedules
{
    public static GlobalDailyScheduleBuilder Daily(IClock clock) => new(clock);
    public static GlobalWeeklyScheduleBuilder Weekly(IClock clock) => new(clock);
    public static GlobalMonthlyScheduleBuilder Monthly(IClock clock) => new(clock);
    public static GlobalOneTimeScheduleBuilder Once(IClock clock) => new(clock);
}

public sealed class GlobalDailyScheduleBuilder
{
    private readonly IClock _clock;
    private readonly List<DailyWindowDefinition> _windows = new();

    internal GlobalDailyScheduleBuilder(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public GlobalDailyScheduleBuilder AddWindow(TimeOfDay start, TimeSpan duration)
    {
        _windows.Add(new DailyWindowDefinition(start, duration));
        return this;
    }

    public GlobalDailyScheduleBuilder Clear()
    {
        _windows.Clear();
        return this;
    }

    public GlobalDailyScheduleBuilder AddWindows(IEnumerable<DailyWindowDefinition> windows)
    {
        if (windows == null) throw new ArgumentNullException(nameof(windows));

        foreach (var window in windows)
        {
            _windows.Add(window);
        }

        return this;
    }

    public GlobalDailyScheduleBuilder AddWindow(int hour, TimeSpan duration)
        => AddWindow(hour, 0, 0, duration);

    public GlobalDailyScheduleBuilder AddWindow(int hour, int minute, TimeSpan duration)
        => AddWindow(hour, minute, 0, duration);

    public GlobalDailyScheduleBuilder AddWindow(int hour, int minute, int second, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        var start = new TimeOfDay(hour, minute, second);
        return AddWindow(start, duration);
    }

    public IGlobalSchedule Build()
    {
        return new GlobalDailySchedule(_clock, _windows);
    }
}

public sealed class GlobalWeeklyScheduleBuilder
{
    private readonly IClock _clock;
    private readonly List<WeeklyWindowDefinition> _definitions = new();

    internal GlobalWeeklyScheduleBuilder(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public GlobalWeeklyScheduleBuilder AddWindow(DayOfWeekFlag days, TimeOfDay start, TimeSpan duration)
    {
        _definitions.Add(new WeeklyWindowDefinition(days, start, duration));
        return this;
    }

    public GlobalWeeklyScheduleBuilder Clear()
    {
        _definitions.Clear();
        return this;
    }

    public GlobalWeeklyScheduleBuilder AddWindows(IEnumerable<WeeklyWindowDefinition> definitions)
    {
        if (definitions == null) throw new ArgumentNullException(nameof(definitions));

        foreach (var definition in definitions)
        {
            _definitions.Add(definition);
        }

        return this;
    }

    public GlobalWeeklyScheduleBuilder AddWindow(DayOfWeekFlag days, int hour, TimeSpan duration)
        => AddWindow(days, hour, 0, 0, duration);

    public GlobalWeeklyScheduleBuilder AddWindow(DayOfWeekFlag days, int hour, int minute, TimeSpan duration)
        => AddWindow(days, hour, minute, 0, duration);

    public GlobalWeeklyScheduleBuilder AddWindow(DayOfWeekFlag days, int hour, int minute, int second, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        var start = new TimeOfDay(hour, minute, second);
        return AddWindow(days, start, duration);
    }

    public IGlobalSchedule Build()
    {
        return new GlobalWeeklySchedule(_clock, _definitions);
    }
}

public sealed class GlobalMonthlyScheduleBuilder
{
    private readonly IClock _clock;
    private readonly List<MonthlyWindowDefinition> _definitions = new();

    internal GlobalMonthlyScheduleBuilder(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public GlobalMonthlyScheduleBuilder AddWindow(int dayOfMonth, TimeOfDay start, TimeSpan duration)
    {
        _definitions.Add(new MonthlyWindowDefinition(dayOfMonth, start, duration));
        return this;
    }

    public GlobalMonthlyScheduleBuilder Clear()
    {
        _definitions.Clear();
        return this;
    }

    public GlobalMonthlyScheduleBuilder AddWindows(IEnumerable<MonthlyWindowDefinition> definitions)
    {
        if (definitions == null) throw new ArgumentNullException(nameof(definitions));

        foreach (var definition in definitions)
        {
            _definitions.Add(definition);
        }

        return this;
    }

    public GlobalMonthlyScheduleBuilder AddWindow(int dayOfMonth, int hour, TimeSpan duration)
        => AddWindow(dayOfMonth, hour, 0, 0, duration);

    public GlobalMonthlyScheduleBuilder AddWindow(int dayOfMonth, int hour, int minute, TimeSpan duration)
        => AddWindow(dayOfMonth, hour, minute, 0, duration);

    public GlobalMonthlyScheduleBuilder AddWindow(int dayOfMonth, int hour, int minute, int second, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        var start = new TimeOfDay(hour, minute, second);
        return AddWindow(dayOfMonth, start, duration);
    }

    public GlobalMonthlyScheduleDayBuilder On(int dayOfMonth)
        => new(this, dayOfMonth);

    public GlobalMonthlyScheduleDayBuilder LastDay()
        => new(this, MonthlyWindowDefinition.LastDay);

    public IGlobalSchedule Build()
    {
        return new GlobalMonthlySchedule(_clock, _definitions);
    }

    public readonly struct GlobalMonthlyScheduleDayBuilder
    {
        private readonly GlobalMonthlyScheduleBuilder _parent;
        private readonly int _dayOfMonth;

        internal GlobalMonthlyScheduleDayBuilder(GlobalMonthlyScheduleBuilder parent, int dayOfMonth)
        {
            _parent     = parent;
            _dayOfMonth = dayOfMonth;
        }

        public GlobalMonthlyScheduleBuilder At(TimeOfDay start, TimeSpan duration)
        {
            return _parent.AddWindow(_dayOfMonth, start, duration);
        }

        public GlobalMonthlyScheduleBuilder At(int hour, TimeSpan duration)
            => At(hour, 0, 0, duration);

        public GlobalMonthlyScheduleBuilder At(int hour, int minute, TimeSpan duration)
            => At(hour, minute, 0, duration);

        public GlobalMonthlyScheduleBuilder At(int hour, int minute, int second, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
            var start = new TimeOfDay(hour, minute, second);
            return At(start, duration);
        }
    }
}

public sealed class GlobalOneTimeScheduleBuilder
{
    private readonly IClock _clock;
    private DateTime? _startUtc;
    private DateTime? _endUtc;

    internal GlobalOneTimeScheduleBuilder(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public GlobalOneTimeScheduleBuilder Between(DateTime startUtc, DateTime endUtc)
    {
        if (startUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Start must be UTC", nameof(startUtc));
        if (endUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("End must be UTC", nameof(endUtc));
        if (endUtc <= startUtc)
            throw new ArgumentException("End must be after start", nameof(endUtc));

        _startUtc = startUtc;
        _endUtc   = endUtc;
        return this;
    }

    public GlobalOneTimeScheduleBuilder StartsAt(DateTime startUtc)
    {
        if (startUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Start must be UTC", nameof(startUtc));
        _startUtc = startUtc;
        if (_endUtc.HasValue && _endUtc.Value <= startUtc)
            _endUtc = null;
        return this;
    }

    public GlobalOneTimeScheduleBuilder EndsAt(DateTime endUtc)
    {
        if (endUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("End must be UTC", nameof(endUtc));
        _endUtc = endUtc;
        return this;
    }

    public GlobalOneTimeScheduleBuilder For(TimeSpan duration)
    {
        if (!_startUtc.HasValue)
            throw new InvalidOperationException("Start time must be specified before setting duration.");
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");

        _endUtc = _startUtc.Value + duration;
        return this;
    }

    public IGlobalSchedule Build()
    {
        if (!_startUtc.HasValue || !_endUtc.HasValue)
            throw new InvalidOperationException("Start and end must be specified for a one-time schedule.");

        return new GlobalOneTimeSchedule(_clock, _startUtc.Value, _endUtc.Value);
    }
}

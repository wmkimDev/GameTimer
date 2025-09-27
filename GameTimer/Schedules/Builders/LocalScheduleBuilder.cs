namespace GameTimer.Schedules.Builders;

using System;
using System.Collections.Generic;
using GameTimer.Abstractions;
using Common;
using Abstractions;
using Models;
using Schedules;

public static class LocalSchedules
{
    public static LocalDailyScheduleBuilder Daily(IClock clock, TimeZoneInfo timeZone, DstPolicy policy = DstPolicy.NextValid)
        => new(clock, timeZone, policy);

    public static LocalWeeklyScheduleBuilder Weekly(IClock clock, TimeZoneInfo timeZone, DstPolicy policy = DstPolicy.NextValid)
        => new(clock, timeZone, policy);

    public static LocalMonthlyScheduleBuilder Monthly(IClock clock, TimeZoneInfo timeZone, DstPolicy policy = DstPolicy.NextValid)
        => new(clock, timeZone, policy);

    public static LocalOneTimeScheduleBuilder Once(IClock clock, TimeZoneInfo timeZone, DstPolicy policy = DstPolicy.NextValid)
        => new(clock, timeZone, policy);
}

public sealed class LocalDailyScheduleBuilder
{
    private readonly IClock _clock;
    private readonly TimeZoneInfo _timeZone;
    private readonly DstPolicy _policy;
    private readonly List<DailyWindowDefinition> _windows = new();

    internal LocalDailyScheduleBuilder(IClock clock, TimeZoneInfo timeZone, DstPolicy policy)
    {
        _clock    = clock ?? throw new ArgumentNullException(nameof(clock));
        _timeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        _policy   = policy;
    }

    public LocalDailyScheduleBuilder AddWindow(TimeOfDay start, TimeSpan duration)
    {
        _windows.Add(new DailyWindowDefinition(start, duration));
        return this;
    }

    public LocalDailyScheduleBuilder Clear()
    {
        _windows.Clear();
        return this;
    }

    public LocalDailyScheduleBuilder AddWindows(IEnumerable<DailyWindowDefinition> windows)
    {
        if (windows == null) throw new ArgumentNullException(nameof(windows));

        foreach (var window in windows)
        {
            _windows.Add(window);
        }

        return this;
    }

    public LocalDailyScheduleBuilder AddWindow(int hour, TimeSpan duration)
        => AddWindow(hour, 0, 0, duration);

    public LocalDailyScheduleBuilder AddWindow(int hour, int minute, TimeSpan duration)
        => AddWindow(hour, minute, 0, duration);

    public LocalDailyScheduleBuilder AddWindow(int hour, int minute, int second, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        var start = new TimeOfDay(hour, minute, second);
        return AddWindow(start, duration);
    }

    public ILocalSchedule Build()
    {
        return new LocalDailySchedule(_clock, _timeZone, _policy, _windows);
    }
}

public sealed class LocalWeeklyScheduleBuilder
{
    private readonly IClock _clock;
    private readonly TimeZoneInfo _timeZone;
    private readonly DstPolicy _policy;
    private readonly List<WeeklyWindowDefinition> _definitions = new();

    internal LocalWeeklyScheduleBuilder(IClock clock, TimeZoneInfo timeZone, DstPolicy policy)
    {
        _clock    = clock ?? throw new ArgumentNullException(nameof(clock));
        _timeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        _policy   = policy;
    }

    public LocalWeeklyScheduleBuilder AddWindow(DayOfWeekFlag days, TimeOfDay start, TimeSpan duration)
    {
        _definitions.Add(new WeeklyWindowDefinition(days, start, duration));
        return this;
    }

    public LocalWeeklyScheduleBuilder Clear()
    {
        _definitions.Clear();
        return this;
    }

    public LocalWeeklyScheduleBuilder AddWindows(IEnumerable<WeeklyWindowDefinition> definitions)
    {
        if (definitions == null) throw new ArgumentNullException(nameof(definitions));

        foreach (var definition in definitions)
        {
            _definitions.Add(definition);
        }

        return this;
    }

    public LocalWeeklyScheduleBuilder AddWindow(DayOfWeekFlag days, int hour, TimeSpan duration)
        => AddWindow(days, hour, 0, 0, duration);

    public LocalWeeklyScheduleBuilder AddWindow(DayOfWeekFlag days, int hour, int minute, TimeSpan duration)
        => AddWindow(days, hour, minute, 0, duration);

    public LocalWeeklyScheduleBuilder AddWindow(DayOfWeekFlag days, int hour, int minute, int second, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        var start = new TimeOfDay(hour, minute, second);
        return AddWindow(days, start, duration);
    }

    public ILocalSchedule Build()
    {
        return new LocalWeeklySchedule(_clock, _timeZone, _policy, _definitions);
    }
}

public sealed class LocalMonthlyScheduleBuilder
{
    private readonly IClock _clock;
    private readonly TimeZoneInfo _timeZone;
    private readonly DstPolicy _policy;
    private readonly List<MonthlyWindowDefinition> _definitions = new();

    internal LocalMonthlyScheduleBuilder(IClock clock, TimeZoneInfo timeZone, DstPolicy policy)
    {
        _clock    = clock ?? throw new ArgumentNullException(nameof(clock));
        _timeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        _policy   = policy;
    }

    public LocalMonthlyScheduleBuilder AddWindow(int dayOfMonth, TimeOfDay start, TimeSpan duration)
    {
        _definitions.Add(new MonthlyWindowDefinition(dayOfMonth, start, duration));
        return this;
    }

    public LocalMonthlyScheduleBuilder Clear()
    {
        _definitions.Clear();
        return this;
    }

    public LocalMonthlyScheduleBuilder AddWindows(IEnumerable<MonthlyWindowDefinition> definitions)
    {
        if (definitions == null) throw new ArgumentNullException(nameof(definitions));

        foreach (var definition in definitions)
        {
            _definitions.Add(definition);
        }

        return this;
    }

    public LocalMonthlyScheduleBuilder AddWindow(int dayOfMonth, int hour, TimeSpan duration)
        => AddWindow(dayOfMonth, hour, 0, 0, duration);

    public LocalMonthlyScheduleBuilder AddWindow(int dayOfMonth, int hour, int minute, TimeSpan duration)
        => AddWindow(dayOfMonth, hour, minute, 0, duration);

    public LocalMonthlyScheduleBuilder AddWindow(int dayOfMonth, int hour, int minute, int second, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        var start = new TimeOfDay(hour, minute, second);
        return AddWindow(dayOfMonth, start, duration);
    }

    public LocalMonthlyScheduleDayBuilder On(int dayOfMonth)
        => new(this, dayOfMonth);

    public LocalMonthlyScheduleDayBuilder LastDay()
        => new(this, MonthlyWindowDefinition.LastDay);

    public ILocalSchedule Build()
    {
        return new LocalMonthlySchedule(_clock, _timeZone, _policy, _definitions);
    }

    public readonly struct LocalMonthlyScheduleDayBuilder
    {
        private readonly LocalMonthlyScheduleBuilder _parent;
        private readonly int _dayOfMonth;

        internal LocalMonthlyScheduleDayBuilder(LocalMonthlyScheduleBuilder parent, int dayOfMonth)
        {
            _parent     = parent;
            _dayOfMonth = dayOfMonth;
        }

        public LocalMonthlyScheduleBuilder At(TimeOfDay start, TimeSpan duration)
        {
            return _parent.AddWindow(_dayOfMonth, start, duration);
        }

        public LocalMonthlyScheduleBuilder At(int hour, TimeSpan duration)
            => At(hour, 0, 0, duration);

        public LocalMonthlyScheduleBuilder At(int hour, int minute, TimeSpan duration)
            => At(hour, minute, 0, duration);

        public LocalMonthlyScheduleBuilder At(int hour, int minute, int second, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
            var start = new TimeOfDay(hour, minute, second);
            return At(start, duration);
        }
    }
}

public sealed class LocalOneTimeScheduleBuilder
{
    private readonly IClock _clock;
    private readonly TimeZoneInfo _timeZone;
    private readonly DstPolicy _policy;
    private DateTime? _startLocal;
    private DateTime? _endLocal;

    internal LocalOneTimeScheduleBuilder(IClock clock, TimeZoneInfo timeZone, DstPolicy policy)
    {
        _clock    = clock ?? throw new ArgumentNullException(nameof(clock));
        _timeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        _policy   = policy;
    }

    public LocalOneTimeScheduleBuilder Between(DateTime startLocal, DateTime endLocal)
    {
        if (startLocal.Kind == DateTimeKind.Utc || endLocal.Kind == DateTimeKind.Utc)
            throw new ArgumentException("Local schedules require local or unspecified DateTime values.");
        if (endLocal <= startLocal)
            throw new ArgumentException("End must be after start.", nameof(endLocal));

        _startLocal = startLocal;
        _endLocal   = endLocal;
        return this;
    }

    public LocalOneTimeScheduleBuilder StartsAt(DateTime startLocal)
    {
        if (startLocal.Kind == DateTimeKind.Utc)
            throw new ArgumentException("Start must be local or unspecified.", nameof(startLocal));
        _startLocal = startLocal;
        if (_endLocal.HasValue && _endLocal.Value <= startLocal)
            _endLocal = null;
        return this;
    }

    public LocalOneTimeScheduleBuilder EndsAt(DateTime endLocal)
    {
        if (endLocal.Kind == DateTimeKind.Utc)
            throw new ArgumentException("End must be local or unspecified.", nameof(endLocal));
        _endLocal = endLocal;
        return this;
    }

    public LocalOneTimeScheduleBuilder For(TimeSpan duration)
    {
        if (!_startLocal.HasValue)
            throw new InvalidOperationException("Start time must be specified before setting duration.");
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");

        _endLocal = _startLocal.Value.Add(duration);
        return this;
    }

    public ILocalSchedule Build()
    {
        if (!_startLocal.HasValue || !_endLocal.HasValue)
            throw new InvalidOperationException("Start and end must be specified for a one-time schedule.");

        return new LocalOneTimeSchedule(_clock, _timeZone, _policy, _startLocal.Value, _endLocal.Value);
    }
}

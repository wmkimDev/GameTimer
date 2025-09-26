namespace GameTimer.Schedules.Schedules;

using System;
using GameTimer.Abstractions;
using Common;
using Abstractions;

internal abstract class LocalScheduleBase : ScheduleBase, ILocalSchedule
{
    private readonly TimeZoneInfo _timeZone;

    protected LocalScheduleBase(IClock clock, TimeZoneInfo timeZone, DstPolicy policy) : base(clock)
    {
        _timeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        Policy    = policy;
    }

    protected TimeZoneInfo ScheduleTimeZone => _timeZone;

    protected DstPolicy Policy { get; }

    public override TimeZoneInfo TimeZone => _timeZone;
}

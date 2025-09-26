namespace GameTimer.Schedules.Schedules;

using System;
using GameTimer.Abstractions;
using Abstractions;

internal abstract class GlobalScheduleBase : ScheduleBase, IGlobalSchedule
{
    protected GlobalScheduleBase(IClock clock) : base(clock)
    {
    }

    public override TimeZoneInfo? TimeZone => null;
}

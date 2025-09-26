namespace GameTimer.Schedules.Abstractions;

using System;
using System.Collections.Generic;
using Models;

/// <summary>
/// 활성 구간을 계산해 주는 스케줄 계약.
/// 구현체는 생성 시 주입된 시계를 이용해 현재 상태를 계산합니다.
/// </summary>
public interface ISchedule
{
    ScheduleSnapshot GetSnapshot();

    ScheduleSnapshot GetSnapshot(DateTime utcNow);

    ScheduleWindow? GetNextWindow();

    ScheduleWindow? GetNextWindow(DateTime lastWindowUtc);

    IEnumerable<ScheduleWindow> EnumerateWindows(DateTime fromUtc, DateTime toUtc);

    TimeZoneInfo? TimeZone { get; }
}

public interface IGlobalSchedule : ISchedule
{
}

public interface ILocalSchedule : ISchedule
{
    new TimeZoneInfo TimeZone { get; }
}

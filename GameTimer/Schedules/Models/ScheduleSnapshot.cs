namespace GameTimer.Schedules.Models;

using System;

/// <summary>
/// 스케줄의 현재 상태와 다음 활성 구간 요약.
/// </summary>
public readonly struct ScheduleSnapshot
{
    public ScheduleSnapshot(
        bool isActive,
        TimeSpan timeUntilStart,
        TimeSpan timeUntilEnd,
        ScheduleWindow? currentWindow,
        ScheduleWindow? nextWindow)
    {
        IsActive        = isActive;
        TimeUntilStart  = ClampToNonNegative(timeUntilStart);
        TimeUntilEnd    = ClampToNonNegative(timeUntilEnd);
        CurrentWindow   = currentWindow;
        NextWindow      = nextWindow;
    }

    public bool IsActive { get; }
    public TimeSpan TimeUntilStart { get; }
    public TimeSpan TimeUntilEnd { get; }
    public ScheduleWindow? CurrentWindow { get; }
    public ScheduleWindow? NextWindow { get; }

    private static TimeSpan ClampToNonNegative(TimeSpan value)
        => value < TimeSpan.Zero ? TimeSpan.Zero : value;
}

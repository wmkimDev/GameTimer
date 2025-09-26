namespace GameTimer.Schedules.Internal;

using System;
using System.Linq;
using GameTimer.Schedules.Models;

internal static class ScheduleOrdering
{
    public static DailyWindowDefinition[] SortDailyWindows(IEnumerable<DailyWindowDefinition> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var windows = source.ToArray();
        Array.Sort(windows, static (left, right) => left.Start.CompareTo(right.Start));
        return windows;
    }

    public static WeeklyWindowDefinition[] SortWeeklyDefinitions(IEnumerable<WeeklyWindowDefinition> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var definitions = source.ToArray();
        Array.Sort(definitions, static (left, right) => left.Start.CompareTo(right.Start));
        return definitions;
    }

    public static MonthlyWindowDefinition[] SortMonthlyDefinitions(IEnumerable<MonthlyWindowDefinition> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var definitions = source.ToArray();
        Array.Sort(definitions, CompareMonthlyDefinitions);
        return definitions;
    }

    private static int CompareMonthlyDefinitions(MonthlyWindowDefinition left, MonthlyWindowDefinition right)
    {
        var leftDay  = NormalizeDay(left.DayOfMonth);
        var rightDay = NormalizeDay(right.DayOfMonth);

        var dayComparison = leftDay.CompareTo(rightDay);
        if (dayComparison != 0)
            return dayComparison;

        return left.Start.CompareTo(right.Start);
    }

    private static int NormalizeDay(int dayOfMonth)
    {
        return dayOfMonth == MonthlyWindowDefinition.LastDay
            ? int.MaxValue
            : dayOfMonth;
    }
}

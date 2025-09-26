namespace GameTimer.Common;

/// <summary>
/// 요일 플래그 (비트 마스크로 여러 요일 조합 가능)
/// </summary>
[Flags]
public enum DayOfWeekFlag : int
{
    /// <summary>미지정</summary>
    None = 0,
    /// <summary>일요일</summary>
    Sunday = 1 << 0,
    /// <summary>월요일</summary>
    Monday = 1 << 1,
    /// <summary>화요일</summary>
    Tuesday = 1 << 2,
    /// <summary>수요일</summary>
    Wednesday = 1 << 3,
    /// <summary>목요일</summary>
    Thursday = 1 << 4,
    /// <summary>금요일</summary>
    Friday = 1 << 5,
    /// <summary>토요일</summary>
    Saturday = 1 << 6,

    // 편의성 조합
    /// <summary>주말 (토요일 + 일요일)</summary>
    Weekend = Saturday | Sunday,
    /// <summary>평일 (월~금)</summary>
    Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,
    /// <summary>모든 요일</summary>
    All = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday
}

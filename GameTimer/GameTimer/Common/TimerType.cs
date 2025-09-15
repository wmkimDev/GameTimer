namespace GameTimer.Common;

/// <summary>
/// 타이머 유형 (메타데이터/직렬화용)
/// </summary>
public enum TimerType
{
    /// <summary>매일 특정 시각</summary>
    Daily,
    /// <summary>매주 특정 요일+시각</summary>
    Weekly,
    /// <summary>고정 간격 반복</summary>
    Interval,
    /// <summary>매월 특정 날짜+시각</summary>
    Monthly,
}
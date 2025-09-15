using GameTimer.Core;

namespace GameTimer.Clocks;

/// <summary>
/// 시스템 시간을 사용하는 실제 시계
/// </summary>
public sealed class SystemClock : IClock
{
    /// <summary>
    /// 현재 시스템의 UTC 시간
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;
}
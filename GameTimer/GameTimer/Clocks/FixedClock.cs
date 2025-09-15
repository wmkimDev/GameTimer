using GameTimer.Core;

namespace GameTimer.Clocks;

/// <summary>
/// 고정된 시간을 반환하는 테스트용 시계
/// </summary>
public sealed class FixedClock : IClock
{
    private DateTime _fixedUtc;

    /// <summary>
    /// 고정 시간 시계 생성
    /// </summary>
    /// <param name="fixedUtc">고정할 UTC 시간</param>
    public FixedClock(DateTime fixedUtc)
    {
        _fixedUtc = DateTime.SpecifyKind(fixedUtc, DateTimeKind.Utc);
    }

    /// <summary>
    /// 고정된 UTC 시간 반환
    /// </summary>
    public DateTime UtcNow => _fixedUtc;

    /// <summary>
    /// 고정 시간 변경 (테스트에서 시간 흐름 시뮬레이션용)
    /// </summary>
    /// <param name="newUtc">새로운 UTC 시간</param>
    public void SetUtc(DateTime newUtc)
    {
        _fixedUtc = DateTime.SpecifyKind(newUtc, DateTimeKind.Utc);
    }

    /// <summary>
    /// 시간을 앞으로 이동 (테스트에서 시간 경과 시뮬레이션용)
    /// </summary>
    /// <param name="timeSpan">이동할 시간</param>
    public void AdvanceBy(TimeSpan timeSpan)
    {
        _fixedUtc = _fixedUtc.Add(timeSpan);
    }
}
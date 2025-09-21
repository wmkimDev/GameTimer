namespace GameTimer.Common;

/// <summary>
/// DST(Daylight Saving Time) 전환 시 무효/모호 시각 처리 정책
/// </summary>
public enum DstPolicy
{
    /// <summary>
    /// 기본값: 무효시각→다음유효시각, 모호시각→표준시 선택
    /// 대부분의 게임 콘텐츠에 안전하며 플레이어 불이익 없음
    /// </summary>
    NextValid,
    
    /// <summary>
    /// 엄격 모드: DST 문제 발생 시 즉시 예외 발생
    /// 중요 시스템(결제, 랭킹)에서 예상치 못한 동작 방지
    /// </summary>
    ThrowException
}
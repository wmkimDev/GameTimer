namespace GameTimer.Abstractions;

/// <summary>
/// 로컬 타임존 기반 타이머 (각 지역별 현지 시간 기준)
/// </summary>
public interface ILocalTimer
{
    // "다음 리셋 언제지?"
    DateTime NextResetUtc(DateTime lastUtc, TimeZoneInfo timeZone);
    // "지금 리셋해야 하나?"
    bool ShouldReset(DateTime lastUtc, TimeZoneInfo timeZone);
    // 다음 리셋까지 남은 시간
    TimeSpan TimeUntilReset(DateTime lastUtc, TimeZoneInfo timeZone);
}

/// <summary>
/// 글로벌 UTC 기반 타이머 (전 세계 동시 실행)
/// </summary>
public interface IGlobalTimer
{
    // "다음 리셋 언제지?"
    DateTime NextResetUtc(DateTime lastUtc);
    // "지금 리셋해야 하나?"
    bool ShouldReset(DateTime lastUtc);
    // 다음 리셋까지 남은 시간
    TimeSpan TimeUntilReset(DateTime lastUtc);
}
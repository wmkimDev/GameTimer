namespace GameTimer.Core;

public interface ITimer
{
    // "다음 리셋 언제지?"
    DateTime NextResetUtc(DateTime lastUtc, TimeZoneInfo timeZone); 
    // "지금 리셋해야 하나?"
    bool ShouldReset(DateTime lastUtc, TimeZoneInfo timeZone);
    // 다음 리셋까지 남은 시간
    TimeSpan TimeUntilReset(DateTime lastUtc, TimeZoneInfo timeZone);
}
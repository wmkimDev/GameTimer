using GameTimer.Abstractions;
using GameTimer.Common;

namespace GameTimer.Timers;

public sealed class LocalOnceAtTimer : LocalTimerBase
{
    private readonly DateTime _targetTimeLocal;

    /// <summary>
    /// 특정 현지시간 시점에 한 번만 실행되는 로컬 타이머 생성
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="targetTimeLocal">
    /// 실행될 현지시간 시점 (DateTimeKind.Unspecified 권장)
    /// <strong>주의:</strong> DateTimeKind.Utc인 경우 예외가 발생합니다. UTC 시간은 GlobalOnceAtTimer를 사용하세요.
    /// </param>
    /// <exception cref="ArgumentException">targetTimeLocal이 UTC 시간인 경우</exception>
    public LocalOnceAtTimer(IClock clock, DateTime targetTimeLocal, DstPolicy policy = DstPolicy.NextValid) : base(clock, policy)
    {
        if (targetTimeLocal.Kind == DateTimeKind.Utc)
            throw new ArgumentException("Use GlobalOnceAtTimer for UTC times", nameof(targetTimeLocal));
            
        _targetTimeLocal = DateTime.SpecifyKind(targetTimeLocal, DateTimeKind.Unspecified);
    }

    /// <summary>
    /// 편의 생성자 - 날짜와 시각을 개별 매개변수로 설정
    /// </summary>
    /// <param name="clock">시계 인스턴스</param>
    /// <param name="year">년도</param>
    /// <param name="month">월 (1-12)</param>
    /// <param name="day">일 (1-31)</param>
    /// <param name="hour">현지시간 시간 (0-23)</param>
    /// <param name="minute">현지시간 분 (0-59)</param>
    /// <param name="second">현지시간 초 (0-59)</param>
    public LocalOnceAtTimer(IClock clock, int year, int month, int day, 
        int hour = 0, int minute = 0, int second = 0, DstPolicy policy = DstPolicy.NextValid)
        : this(clock, new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified), policy)
    {
    }

    public override DateTime NextResetUtc(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));
        
        // DST 인식 안전한 변환 사용
        return DstHelpers.SafeConvertToUtc(_targetTimeLocal, timeZone, Policy);
    }

    public override bool ShouldReset(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));
        
        var lastLocal = TimeZoneInfo.ConvertTimeFromUtc(lastUtc, timeZone);
        
        if (lastLocal >= _targetTimeLocal) return false;
        
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(Clock.UtcNow, timeZone);
        return nowLocal >= _targetTimeLocal.Subtract(Latency);
    }

    public override TimeSpan TimeUntilReset(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));
        
        var lastLocal = TimeZoneInfo.ConvertTimeFromUtc(lastUtc, timeZone);
        
        // 이미 실행되었으면 0 반환
        if (lastLocal >= _targetTimeLocal) return TimeSpan.Zero;
        
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(Clock.UtcNow, timeZone);
        var remaining = _targetTimeLocal - nowLocal;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// 설정된 실행 시점 (현지시간)
    /// </summary>
    public DateTime TargetTimeLocal => _targetTimeLocal;

    /// <summary>
    /// 타이머가 이미 실행되었는지 확인
    /// </summary>
    /// <param name="lastUtc">마지막 확인 시간 (UTC)</param>
    /// <param name="timeZone">타임존 정보</param>
    /// <returns>이미 실행되었는지 여부</returns>
    public bool HasExecuted(DateTime lastUtc, TimeZoneInfo timeZone)
    {
        TimerHelpers.ValidateUtc(lastUtc, nameof(lastUtc));
        if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));
        
        var lastLocal = TimeZoneInfo.ConvertTimeFromUtc(lastUtc, timeZone);
        return lastLocal >= _targetTimeLocal;
    }
}

namespace GameTimer.Common;

/// <summary>
/// DST 전환을 고려한 안전한 시각 변환 헬퍼
/// </summary>
public static class DstHelpers
{
    /// <summary>
    /// DST 전환을 고려한 안전한 UTC 변환
    /// </summary>
    /// <param name="localTime">변환할 현지시간 (DateTimeKind.Unspecified)</param>
    /// <param name="timeZone">타임존 정보</param>
    /// <param name="policy">DST 처리 정책</param>
    /// <returns>UTC 시간</returns>
    public static DateTime SafeConvertToUtc(DateTime localTime, TimeZoneInfo timeZone, DstPolicy policy = DstPolicy.NextValid)
    {
        if (timeZone == null) 
            throw new ArgumentNullException(nameof(timeZone));
        
        // 무효 시각 처리 (봄 전환: 존재하지 않는 시각)
        if (timeZone.IsInvalidTime(localTime))
        {
            switch (policy)
            {
                case DstPolicy.NextValid:
                    return FindNextValidTime(localTime, timeZone);
                    
                case DstPolicy.ThrowException:
                    throw new InvalidTimeZoneException(
                        $"Invalid time during DST transition: {localTime:yyyy-MM-dd HH:mm:ss} in {timeZone.Id}");
                        
                default:
                    throw new ArgumentException($"Unsupported DST policy: {policy}");
            }
        }
        
        // 모호 시각 처리 (가을 전환: 두 번 발생하는 시각)
        if (timeZone.IsAmbiguousTime(localTime))
        {
            switch (policy)
            {
                case DstPolicy.NextValid:
                    // .NET 기본 동작: 모호한 시각을 표준시로 가정 (Microsoft 공식 문서)
                    return TimeZoneInfo.ConvertTimeToUtc(localTime, timeZone);
                    
                case DstPolicy.ThrowException:
                    throw new ArgumentException(
                        $"Ambiguous time during DST transition: {localTime:yyyy-MM-dd HH:mm:ss} in {timeZone.Id}");
                        
                default:
                    throw new ArgumentException($"Unsupported DST policy: {policy}");
            }
        }
        
        // 정상 시각: 일반 변환
        return TimeZoneInfo.ConvertTimeToUtc(localTime, timeZone);
    }
    
    /// <summary>
    /// 무효 시각 이후 첫 번째 유효한 시각 찾기
    /// </summary>
    private static DateTime FindNextValidTime(DateTime invalidTime, TimeZoneInfo timeZone)
    {
        var candidate = invalidTime;
        
        // 분 단위로 전진하며 유효한 시각 찾기 (최대 3시간)
        for (var minutes = 1; minutes <= 180; minutes++)
        {
            candidate = invalidTime.AddMinutes(minutes);
            
            if (!timeZone.IsInvalidTime(candidate))
            {
                return TimeZoneInfo.ConvertTimeToUtc(candidate, timeZone);
            }
        }
        
        throw new InvalidOperationException(
            $"Could not find valid time after DST transition for {invalidTime:O} in {timeZone.Id}");
    }
}
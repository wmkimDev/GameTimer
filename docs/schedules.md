# Schedules

스케줄은 반복되는 활성 구간(`ScheduleWindow`)을 계산하는 고수준 API입니다. 게임 이벤트, 배치 작업, 컨텐츠 개방 시간 등을 정의할 때 사용합니다.

## 구성요소
- `ISchedule`: 스케줄들의 기본 인터페이스. `GetSnapshot`, `GetNextWindow`, `EnumerateWindows`, `TimeZone` 등을 제공합니다.
- `ScheduleWindow`: UTC 기준 시작/종료 시간을 담는 불변 구조체. `Contains`, `Intersects` 유틸리티를 제공합니다.
- `ScheduleSnapshot`: 현재 활성 여부와 다음 창까지 남은 시간을 포함한 스냅샷.

## 제공 빌더
| 빌더 | 설명 |
| ---- | ---- |
| `GlobalSchedules.Daily` | UTC 기준 하루 여러 창 정의 |
| `GlobalSchedules.Weekly` | 특정 요일 + 시각 조합 |
| `GlobalSchedules.Monthly` | 특정 일자 또는 말일 |
| `GlobalSchedules.Once` | 한 번만 발생하는 창 |
| `LocalSchedules.Daily` | 타임존 기준 하루 창 |
| `LocalSchedules.Weekly` | 요일/시각 + 타임존 |
| `LocalSchedules.Monthly` | 일자/말일 + 타임존 |
| `LocalSchedules.Once` | 타임존 기준 단발성 |

## 사용 예

```csharp
var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
var daily = LocalSchedules.Daily(clock, tz)
    .AddWindow(9, 0, duration: TimeSpan.FromHours(2))
    .AddWindow(19, 0, duration: TimeSpan.FromHours(1))
    .Build();

var snapshot = daily.GetSnapshot();
if (snapshot.IsActive)
{
    Console.WriteLine($"현재 창 종료까지: {snapshot.TimeUntilEnd}");
}
else if (snapshot.NextWindow.HasValue)
{
    Console.WriteLine($"다음 창 시작까지: {snapshot.TimeUntilStart}");
}
```

## 창 열거
```csharp
DateTime start = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
DateTime end   = new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc);

foreach (var window in daily.EnumerateWindows(start, end))
{
    Console.WriteLine($"{window.StartUtc:O} ~ {window.EndUtc:O}");
}
```
`EnumerateWindows` 호출 시 `toUtc`는 `fromUtc`보다 커야 하며, 창이 실제로 범위를 교차할 때만 반환됩니다.

## DST 고려
로컬 스케줄은 생성 시 `TimeZoneInfo`와 `DstPolicy`를 필수로 받습니다. 내부적으로 `DstHelpers.SafeConvertToUtc`를 이용해 모호/존재하지 않는 시각을 안전하게 처리합니다.

- `NextValid`: 존재하지 않는 시각은 다음 유효 시각으로 이동, 모호 시각은 표준시로 간주.
- `ThrowException`: 문제 발견 시 즉시 예외를 던져 사용자에게 알립니다.

## 말일/여러 일자 조합
```csharp
var monthly = LocalSchedules.Monthly(clock, tz)
    .On(5).At(9, 0)
    .On(20).At(18, 0)
    .LastDay().At(6, 0)
    .Build();
```
스케줄 빌더는 내부적으로 창 목록을 정렬하여 `GetNextWindow` 호출 시 불필요한 정렬 비용이 들지 않도록 합니다.

## 단발성 스케줄
```csharp
var oneTime = GlobalSchedules.Once(clock)
    .StartsAt(DateTime.UtcNow.AddHours(1))
    .For(TimeSpan.FromHours(2))
    .Build();

var window = oneTime.GetNextWindow(DateTime.UtcNow);
```
One-time 스케줄은 단일 창만 반환하며, 종료 후에는 `GetNextWindow`가 `null`을 반환합니다.

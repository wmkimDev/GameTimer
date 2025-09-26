# DST & Time Zones

GameTimer는 로컬 타이머/스케줄을 계산할 때 `TimeZoneInfo`를 기반으로 안전하게 UTC 변환을 수행합니다. 이 문서는 DST 정책과 관련된 주의사항을 정리합니다.

## DstPolicy
- `NextValid` (기본)
  - 존재하지 않는 시각(봄 전환) → 다음 유효 시각으로 이동
  - 모호한 시각(가을 전환) → .NET 기본 규칙(표준시 우선)을 그대로 사용
- `ThrowException`
  - 위의 상황이 발생하면 즉시 예외를 던집니다. 정교한 핸들링이 필요할 때 사용하세요.

## 예제: 봄 전환
```csharp
var la = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
var schedule = LocalSchedules.Once(clock, la, DstPolicy.NextValid)
    .Between(TestHelpers.Unspec(2025, 3, 9, 2, 30, 0), TestHelpers.Unspec(2025, 3, 9, 3, 30, 0))
    .Build();
// 02:30은 존재하지 않지만 NextValid 정책이라 03:00(UTC 10:00)으로 이동하여 창을 생성합니다.
```
`ThrowException`으로 설정하면 위 케이스는 `InvalidTimeZoneException`을 발생시킵니다.

## 예제: 가을 전환
```csharp
var la = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
var schedule = LocalSchedules.Daily(clock, la, DstPolicy.ThrowException)
    .AddWindow(new TimeOfDay(1, 30), TimeSpan.FromHours(1))
    .Build();
// 1시 30분이 두 번 존재하는 날에는 예외가 발생합니다.
```
`NextValid` 정책이라면 1차 표준시 구간으로 매핑된 창이 생성됩니다.

## 모범 사례
1. **입력 형식 명확화**: 로컬 스케줄/타이머에 UTC `DateTime`을 넣지 마세요. `DateTimeKind.Unspecified` 사용을 권장합니다.
2. **테스트 작성**: DST 경계(봄/가을)와 말일 처리 등 경계 케이스를 `FixedClock`과 함께 테스트하세요.
3. **예외 처리**: `ThrowException`을 선택했다면, 호출부에서 예외를 받아 사용자에게 명확히 안내하거나 대체 시나리오를 마련하세요.

## TimeZoneInfo 해상도
테스트 환경에 따라 IANA/Windows ID가 다를 수 있습니다. 예제 코드에서는 `TestHelpers.Tz("Asia/Seoul", "Korea Standard Time")` 처럼 복수의 ID를 시도하는 헬퍼를 제공합니다.

GameTimer
=========

경량 .NET 타이머 유틸리티. 글로벌(UTC)과 로컬(타임존) 기반의 다양한 리셋/간격 타이머를 안전한 API로 제공합니다. DST 처리 정책, 테스트 가능한 Clock 추상화를 포함합니다.

특징
- 글로벌(UTC) 타이머와 로컬(타임존) 타이머 분리
- 타이머 종류: OnceAt(1회), EveryDay/EveryWeek/EveryMonth, MultipleTimes(하루 여러 시각), Interval(고정 간격), AfterDuration(지속 시간 후 1회)
- DST 정책 선택: NextValid(기본), ThrowException
- 테스트용 `FixedClock`, 실제용 `SystemClock`
- `TimeOfDay` 구조체로 시각을 안전하게 표현

빠른 시작
1) Clock 준비
```csharp
using GameTimer.Clocks;
var clock = new SystemClock();
```

2) 글로벌(UTC) 매일 00:00 리셋
```csharp
using GameTimer.Builder;
using GameTimer.Timers;

var daily = GlobalTimers.Daily(clock)
    .UtcAt(0) // 00:00 UTC
    .Build();

DateTime lastUtc = DateTime.UtcNow.AddHours(-1);
DateTime next    = daily.NextResetUtc(lastUtc);
bool should      = daily.ShouldReset(lastUtc);
```

3) 로컬(타임존) 주말 06:00 리셋 (DST 안전)
```csharp
using GameTimer.Builder;
using GameTimer.Common;

var weekly = LocalTimers.Weekly(clock, DstPolicy.NextValid)
    .On(DayOfWeekFlag.Weekend)
    .At(6) // 06:00 (로컬 의미)
    .Build();

var tz   = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
var last = DateTime.UtcNow.AddDays(-1);
DateTime nextLocalUtc = weekly.NextResetUtc(last, tz);
bool shouldLocal      = weekly.ShouldReset(last, tz);
```

4) 간격/지속시간 기반
```csharp
using GameTimer.Timers;

var every2h   = new IntervalTimer(clock, TimeSpan.FromHours(2));
var finishes5 = new AfterDurationTimer(clock, TimeSpan.FromMinutes(5));
```

중요 개념
- 글로벌 vs 로컬
  - 글로벌 타이머는 UTC를 직접 사용합니다.
  - 로컬 타이머는 특정 타임존에서의 “현지 시각” 기준으로 동작하며, 내부에서 안전한 UTC 변환을 수행합니다.

- DateTimeKind 처리
  - 로컬 입력 시각은 `DateTimeKind.Unspecified`를 권장합니다.
  - `LocalOnceAtTimer`에 UTC `DateTime`을 전달하면 예외가 발생합니다. UTC는 `GlobalOnceAtTimer`를 사용하세요.

- DST(Daylight Saving Time)
  - `DstPolicy.NextValid`(기본): 존재하지 않는 시각은 다음 유효 시각으로 보정, 모호 시각은 .NET 기본 규칙을 따릅니다.
  - `DstPolicy.ThrowException`: DST 전환 문제 발생 시 예외를 던집니다.

- 지연 허용치(Latency)
  - 모든 타이머는 `Latency`(기본 2초)를 두고, `ShouldReset` 평가 시 약간의 버퍼를 둡니다.
  - 예: `timer.Latency = TimeSpan.FromSeconds(5);`


# Timers

GameTimer는 주기적인 리셋 로직을 간단하게 구성할 수 있는 타이머 빌더를 제공합니다. 모든 타이머는 `IClock`을 생성자에서 받아 현재 시간을 평가하며, `ShouldReset`, `NextResetUtc` 같은 메서드를 통해 사용합니다.

## 공통 개념
- `Latency`: 기본 2초 버퍼. `ShouldReset` 평가 시 마지막 실행 이후 이 시간만큼은 허용 오차로 간주합니다. 필요 시 `timer.Latency = TimeSpan.FromSeconds(5);` 처럼 수정합니다.
- `NextResetUtc(DateTime lastUtc)`는 마지막 실행 시각을 전달하고 다음 리셋 시각을 반환합니다.
- 로컬 타이머의 경우 `NextResetUtc(lastUtc, timeZone)`와 같이 타임존을 함께 넘겨야 합니다.

## 글로벌 타이머
UTC를 기준으로 동작합니다.

```csharp
var daily = GlobalTimers.Daily(clock)
    .UtcAt(0)           // 매일 00:00 UTC
    .WithLatency(TimeSpan.FromSeconds(3))
    .Build();
```

### 지원 빌더
| 빌더 | 설명 |
| ---- | ---- |
| `Daily` | 하루에 한 번 UTC 기준 시각에 리셋 |
| `Weekly` | 특정 요일 + 시각 조합 |
| `Monthly` | 특정 일자 + 시각 조합, `LastDay()` 지원 |
| `MultipleTimes` | 하루 여러 시각을 한 번에 등록 |
| `OnceAt` | 지정 시각에 한 번 리셋 |

## 로컬 타이머
현지 시간을 기준으로 동작하며, DST 정책과 타임존에 따라 UTC로 변환됩니다.

```csharp
var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
var weekly = LocalTimers.Weekly(clock)
    .WithPolicy(DstPolicy.NextValid)
    .On(DayOfWeekFlag.Weekend)
    .At(6, 0)
    .Build();

var next = weekly.NextResetUtc(lastUtc, tz);
```

### 정책 변경 예
```csharp
var strict = LocalTimers.Once(clock)
    .WithPolicy(DstPolicy.ThrowException)
    .OnceAt(TestHelpers.Unspec(2025, 3, 30, 2, 30));
```
DST 중에 존재하지 않는 시각을 쓰면 `ThrowException` 정책에서 즉시 예외가 보고됩니다.

## 간격/지속 타이머
| 타입 | 설명 |
| ---- | ---- |
| `IntervalTimer` | 고정 간격 반복. 마지막 실행 시각을 내부 상태로 갱신해야 한다면 래퍼나 캐시를 사용하세요. |
| `AfterDurationTimer` | 특정 기간 후 한 번만 발동. |

```csharp
var interval = new IntervalTimer(clock, TimeSpan.FromMinutes(30));
var after5   = new AfterDurationTimer(clock, TimeSpan.FromMinutes(5));
```

## 타이머 vs 스케줄
타이머는 “다음 리셋이 언제인가?”에 초점을 맞추고, 스케줄은 “활성 기간이 언제인가?”에 초점을 둡니다. 타이머는 간단한 플래그(True/False)와 다음 시각을 주고, 스케줄은 `ScheduleSnapshot`으로 현재/다음 창, 남은 시간 등의 정보를 제공합니다.

GameTimer
=========

GameTimer는 게임/실시간 서비스에서 반복적으로 발생하는 이벤트를 안전하게 다룰 수 있도록 설계된 .NET 타이머 & 스케줄 라이브러리입니다. 글로벌(UTC)과 로컬(타임존) 시나리오를 모두 지원하며, DST(서머타임) 전환과 테스트 가능성을 고려한 API를 제공합니다.

## 목차
- [주요 특징](#주요-특징)
- [설치 및 준비](#설치-및-준비)
- [핵심 개념](#핵심-개념)
  - [Clock 추상화](#clock-추상화)
  - [DST 정책](#dst-정책)
- [타이머 사용법](#타이머-사용법)
  - [글로벌 타이머](#글로벌-타이머)
  - [로컬 타이머](#로컬-타이머)
  - [간격/지속 기반 타이머](#간격지속-기반-타이머)
- [스케줄 사용법](#스케줄-사용법)
  - [스케줄 스냅샷](#스케줄-스냅샷)
  - [글로벌 스케줄](#글로벌-스케줄)
  - [로컬 스케줄](#로컬-스케줄)
  - [One-time 스케줄](#one-time-스케줄)
  - [창(Windows) 열거](#창windows-열거)
- [예제 시나리오](#예제-시나리오)
- [테스트](#테스트)
- [추가 문서](#추가-문서)

## 주요 특징
- **명확한 시간 구분**: 글로벌(UTC) / 로컬(타임존) API가 분리되어 실수 가능성을 줄입니다.
- **다양한 타이머**: OnceAt, EveryDay/Week/Month, MultipleTimes, Interval, AfterDuration 등.
- **스케줄 빌더**: 반복되는 활성 구간을 정의하고 현재/다음 창을 계산할 수 있습니다.
- **DST 대응**: `DstPolicy`를 통해 서머타임 전환 시 문제를 처리할 방식을 선택할 수 있습니다.
- **테스트 용이성**: `IClock` 추상화와 `FixedClock`으로 시간을 고정해 단위 테스트를 쉽게 작성합니다.

## 설치 및 준비
프로젝트 레퍼런스로 추가하여 사용합니다. (NuGet 패키지를 배포하지 않는 상태이므로 소스나 프로젝트 레퍼런스를 직접 연결하세요.)

```bash
# 예시: 동일 솔루션 내에서 GameTimer.csproj 참조 추가
dotnet add <your-project>.csproj reference GameTimer/GameTimer.csproj
```

## 핵심 개념

### Clock 추상화
```csharp
using GameTimer.Clocks;

IClock systemClock = new SystemClock();      // 실제 시스템 시간
IClock fixedClock  = new FixedClock(DateTime.UtcNow); // 테스트용 고정 시간
```
모든 타이머/스케줄 생성 시 `IClock`을 전달해야 하며, 이를 통해 테스트와 실제 실행 환경을 분리할 수 있습니다.

### DST 정책
```csharp
using GameTimer.Common;

var policy = DstPolicy.NextValid;      // 기본: 존재하지 않는 시각은 다음 유효 시각으로 보정
// var policy = DstPolicy.ThrowException; // 엄격 모드: 전환 문제 발견 시 즉시 예외
```
로컬 타이머/스케줄은 DST 정책에 따라 모호하거나 존재하지 않는 시각을 처리합니다.

## 타이머 사용법

### 글로벌 타이머
UTC 기반으로 동작하며 타임존을 고려하지 않습니다.
```csharp
using GameTimer.Builder;

var dailyUtc = GlobalTimers.Daily(systemClock)
    .UtcAt(0)    // 매일 00:00 UTC
    .Build();

DateTime last = DateTime.UtcNow.AddHours(-3);
DateTime nextReset = dailyUtc.NextResetUtc(last);
bool shouldReset = dailyUtc.ShouldReset(last);
```

### 로컬 타이머
특정 타임존 시각을 기준으로 동작합니다.
```csharp
using GameTimer.Common;

var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
var weeklyLocal = LocalTimers.Weekly(systemClock)
    .WithPolicy(DstPolicy.NextValid)
    .On(DayOfWeekFlag.Monday | DayOfWeekFlag.Wednesday)
    .At(6, 0)    // 06:00 (타임존 기준)
    .Build();

DateTime lastUtc = DateTime.UtcNow.AddDays(-1);
DateTime next = weeklyLocal.NextResetUtc(lastUtc, tz);
bool should = weeklyLocal.ShouldReset(lastUtc, tz);
```

### 간격/지속 기반 타이머
```csharp
using GameTimer.Timers;

var interval = new IntervalTimer(systemClock, TimeSpan.FromMinutes(30));
var onceAfter = new AfterDurationTimer(systemClock, TimeSpan.FromMinutes(5));
```
`IntervalTimer`는 마지막 실행 이후 고정 간격으로 동작하며, `AfterDurationTimer`는 지정 기간 후 한 번만 트리거됩니다.

## 스케줄 사용법
스케줄은 활성 구간(`ScheduleWindow`)을 계산하는 데 초점을 둡니다. 타이머와 달리 현재 창과 다음 창, 남은 시간 등을 상세히 조회할 수 있습니다.

```csharp
using GameTimer.Schedules.Models;

ScheduleSnapshot snapshot = schedule.GetSnapshot();
bool isActive = snapshot.IsActive;
ScheduleWindow? current = snapshot.CurrentWindow;
ScheduleWindow? next = snapshot.NextWindow;
```

### 스케줄 스냅샷
`schedule.GetSnapshot()`은 현재 시간 기준 활성 여부, 다음 시작/종료까지 남은 시간을 제공합니다.

### 글로벌 스케줄
UTC 기준 반복 구간을 정의합니다.
```csharp
using GameTimer.Schedules.Builders;

var globalSchedule = GlobalSchedules.Daily(systemClock)
    .AddWindow(9, 0, TimeSpan.FromHours(1))        // 09:00 UTC, 1시간 창
    .AddWindow(new TimeOfDay(18, 0), TimeSpan.FromHours(2))
    .Build();

var nextWindow = globalSchedule.GetNextWindow(DateTime.UtcNow);
```

### 로컬 스케줄
타임존과 DST 정책을 고려한 구간 계산을 제공합니다.
```csharp
var seoul = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
var localSchedule = LocalSchedules.Monthly(systemClock, seoul)
    .LastDay()                                // 말일
    .At(6, 0, 0, TimeSpan.FromHours(1))       // 06:00 현지 시각, 1시간 창
    .Build();
```
`LocalSchedules.Monthly`는 `On(5).At(9, 0, TimeSpan.FromHours(1))`처럼 특정 일자/시각과 창 길이를 지정할 수도 있고, `.LastDay().At(...)`로 말일 동작을 간단히 정의할 수 있습니다.

### One-time 스케줄
```csharp
var localOnce = LocalSchedules.Once(systemClock, seoul)
    .Between(new DateTime(2025, 12, 25, 8, 0, 0, DateTimeKind.Unspecified),
             new DateTime(2025, 12, 25, 12, 0, 0, DateTimeKind.Unspecified))
    .Build();

var globalOnce = GlobalSchedules.Once(systemClock)
    .StartsAt(DateTime.UtcNow.AddHours(1))
    .For(TimeSpan.FromHours(2))
    .Build();
```

### 창(Windows) 열거
일정 기간 동안의 모든 활성 구간을 얻을 수 있습니다.
```csharp
DateTime fromUtc = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);
DateTime toUtc   = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

foreach (var window in localSchedule.EnumerateWindows(fromUtc, toUtc))
{
    Console.WriteLine($"Start: {window.StartUtc:O}, End: {window.EndUtc:O}");
}
```
`EnumerateWindows`는 열린-닫힌 구간 `(fromUtc, toUtc]`에 해당하는 창만 돌려줍니다. 종료 시간이 시작 시간 이하이면 `ArgumentException`이 발생합니다.

## 예제 시나리오
- **크리스마스 이벤트**: `LocalSchedules.Monthly(clock, tz).On(25).At(0, 0, TimeSpan.FromHours(1))` 혹은 `.LastDay()`를 사용해 반복 이벤트를 구성하고, 12월 기간만 열거하여 필요한 윈도우만 추출합니다.
- **매일 다중 점검**: 글로벌/로컬 `MultipleTimes` 빌더로 하루 여러 시각에 점검을 등록합니다.
- **DST 전환 검증**: `DstPolicy.ThrowException`을 사용해 문제 발생 시 즉시 감지하거나, `NextValid` 정책으로 안전하게 보정합니다.

## 테스트
단위 테스트 프로젝트는 `GameTimer.Test`입니다.

```bash
cd GameTimer.Test
# dotnet CLI가 설치된 환경에서 실행
dotnet test
```
이 저장소에서는 `FixedClock`을 활용해 DST 경계, 말일 처리, 정렬 보장 등 다양한 케이스를 검증하고 있습니다.

## 추가 문서
- [docs/timers.md](docs/timers.md): 세부 타이머 빌더 사용법과 옵션
- [docs/schedules.md](docs/schedules.md): 스케줄 정의, 창 계산 로직, DST 취급에 대한 심화 설명
- [docs/dst-and-timezones.md](docs/dst-and-timezones.md): DST 정책 및 타임존 처리 가이드

필요한 내용이 문서에 없다면 이슈를 등록하거나 Pull Request를 통해 기여해 주세요.

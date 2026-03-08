# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run (listens on http://0.0.0.0:5100)
dotnet run

# Check for warnings/errors
dotnet build --no-incremental 2>&1 | grep -E "(warning|error|CS|BL)"
```

There are no automated tests in this project.

## Architecture

**FitnessPT** is a Blazor Server (.NET 8) frontend that communicates with a separate backend API (default: `http://localhost:5117`, configured via `appsettings.json` → `FITNESSPT:ApiSettings:BaseUrl`).

### Layered Architecture

```
Razor Components (Pages/Modals/Shared)
    ↓ inject
Controllers (Components/Controllers/)   ← thin orchestration + JS interop (alerts/confirms)
    ↓ inject
Services (Services/)                    ← business logic, interface-backed
    ↓ inject
ApiClient (Services/Http/ApiClient.cs)  ← generic HTTP wrapper returning ApiResponse<T>
    ↓
External REST API
```

**Key pattern**: Pages inject `ExerciseController` or `RoutineController` (not services directly). Controllers call services and handle JS dialogs (`alert`, `confirm`). Services call `IApiClient` methods.

### Service Registration (Program.cs)

All services are `Scoped`:
- `IApiClient` → `ApiClient`
- `IExerciseService` → `ExerciseService`
- `IRoutineService` → `RoutineService`
- `ISession` → `SessionService` (wraps `ProtectedSessionStorage`)
- `AuthService`, `ExerciseController`, `RoutineController`, `AlertService`

### Key Directories

- `Components/Pages/` — Blazor pages (`Exercises/`, `Routine/`, `Home`, `Login`)
- `Components/Modals/` — Modal components; complex modals use code-behind `.razor.cs` files
- `Components/Shared/` — Reusable components (`CategoryFillter.razor`, `ExerciseSelecter.razor`)
- `Components/Controllers/` — Controller classes injected by pages; not ASP.NET controllers
- `Services/` — Service interfaces + implementations; `Http/` contains `ApiClient`, `IApiClient`, `ApiResponse<T>`
- `Dtos/` — Data transfer objects (`ExerciseDto`, `RoutineDto`, `RoutineExerciseDto`, `RoutineInfoDto`, `RoutineUpdateDto`)
- `Models/` — `PagedResult<T>`, `User`

### Nullable Reference Types

The project has `<Nullable>enable</Nullable>`. Non-nullable string properties on DTOs/models should be initialized with `= null!;` or `= string.Empty;`. List properties should use `= new();`. This is the cause of most CS8618 warnings.

### API Communication Pattern

`ApiClient` returns `ApiResponse<T>` (with `IsSuccess`, `Data`, `ErrorMessage`). Services unwrap this and throw on failure. Controllers catch exceptions and show JS alerts.

### Session / Auth

`SessionService` wraps Blazor's `ProtectedSessionStorage`. `AuthService` manages login state and exposes `OnAuthStateChanged` event (subscribed by `NavMenu`). Admin actions are gated by a 4-digit PIN modal (`PasswordModal`).

## Analysis Cache
- 코드를 읽거나 분석할 때 docs/claude/ 폴더에서 기존 분석 파일을 먼저 확인
- 분석 파일이 있으면 해당 내용을 기반으로 작업 (원본 코드 재읽기 최소화)
- 분석 파일이 없으면 원본 코드를 읽고 분석 후 자동 저장
- 코드 수정 시 해당 파일의 분석 문서가 있으면 변경 부분도 함께 갱신
- 분석 파일 형식:
    - 파일 목적
    - 메서드 목록 (이름, 라인 범위, 설명)
    - 주요 의존성
    - 주의사항
    - 마지막 분석일시
- 저장 경로: docs/claude/{파일명}_분석.md
```

핵심 변경은 **"확인 후 저장" → "자동 저장"**이야. 이러면:
```
세션 1: InsertInvoiceAPI 분석해줘
→ 원본 읽기 → 분석 → 자동 저장 (토큰 많이 사용)

세션 2: InsertInvoiceAPI에 파일 처리 추가해줘  
→ 분석 파일만 읽기 → 바로 작업 (토큰 절약)

세션 3: InsertInvoiceAPI 수정했는데 리뷰해줘
→ 분석 파일 + 변경 부분만 읽기 → 분석 파일 갱신 (토큰 절약)

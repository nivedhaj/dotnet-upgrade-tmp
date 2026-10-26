# Ct.Scheduler Tests

MSTest suite for **Ct.Scheduler** using **EF Core + Postgres**.

## Coverage map

| Original | New |
|----------|-----|
| CalendarTests (get/create/remove rules, generate, remove occurrence) | `CalendarTests.cs` |
| SchedulerTests (holidays, library due date, next working days, academic-year edge) | `SchedulerTests.cs` |
| ReportTests (holiday list, calendar HTML/model) | `ReportTests.cs` |
| CronTests (weekly/weekday/nth-dow helpers + rule expansion) | `CronTests.cs` |
| IndexTests / PingTests | `IndexTests.cs`, `PingTests.cs` |

## Run

```bash
createdb -U ct ct_modular_test
dotnet test path/to/ct.scheduler.tests.csproj
export CT_TEST_POSTGRES="Host=postgres;Database=ct_modular_test;Username=ct;Password=secret"
dotnet test
```

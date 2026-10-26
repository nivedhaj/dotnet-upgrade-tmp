# Ct.Fee Tests

Full MSTest suite for **Ct.Fee** (EF Core + Postgres), covering the legacy `ct.fee.tests` surface.

## Coverage map

| Original | New |
|----------|-----|
| FeeTests (create/get/update/enable/disable/due date/sanitize/dates/expected) | `FeeTests.cs` |
| FeeTxTests (IsTxDone, list, HTML, details, search, invalid, cancel, balances, receivable, …) | `FeeTxTests.cs` |
| ReportTests (all major reports) | `ReportTests.cs` |
| Refund / Suspense HTTP flows | `RefundTests.cs`, `SuspenseTests.cs` |
| EtlTests / EtlJsonlTests | `EtlTests.cs` (job publish surface) |
| IndexTests | `IndexTests.cs` (EF unique indexes + tenant filters) |
| PingTests | `PingTests.cs` |
| Policy engines | `PolicyTests.cs` |

## Run

```bash
createdb -U ct ct_modular_test
dotnet test path/to/ct.fee.tests.csproj

export CT_TEST_POSTGRES="Host=postgres;Database=ct_modular_test;Username=ct;Password=secret"
dotnet test
```

Each test uses a unique `OwnerId` and cleans up its rows.

# Ct.Staff Tests

MSTest suite for **Ct.Staff** using **EF Core + Postgres**.

## Coverage map

| Original | New |
|----------|-----|
| StaffTests (qry, facet, mobiles, SMS recipients, employment, teaching list, counts, user ref, absentees) | `StaffTests.cs` |
| ReportTests (payslip) + salary structure | `ReportTests.cs` |
| EtlTests (employ to new inst, demo, import, code update) | `EtlTests.cs` |
| IndexTests / PingTests | `IndexTests.cs`, `PingTests.cs` |

## Run

```bash
createdb -U ct ct_modular_test
dotnet test path/to/ct.staff.tests.csproj
export CT_TEST_POSTGRES="Host=...;Database=ct_modular_test;..."
dotnet test
```

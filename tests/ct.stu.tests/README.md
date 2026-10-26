# Ct.Stu Tests

MSTest suite for **Ct.Stu** (students / admissions) using **EF Core + Postgres** (`ct` schema).

## Coverage

Extended suite maps the legacy `ct.stu.tests` surface onto modular services:

| Area | Files |
|------|--------|
| Student CRUD, status, TC, promote, course/section, tags, contacts, counts, jobs-style cmds, data-fix | `StudentTests.cs`, `StudentTests.Extended.cs` |
| Reports (section/course, parents, CSV, new admissions, TC, IEP marker) | `ReportTests.cs`, `ReportTests.Extended.cs` |
| ETL (CSV, RTE, promote, import, virtual id, fee-txn list, opening balance marker) | `EtlTests.cs`, `EtlTests.Extended.cs` |
| Email recipients / parent email export (campaigns stay on Connect host) | `EmailTests.cs`, `EmailTests.Extended.cs` |
| Indexes / ping | `IndexTests*.cs`, `PingTests.cs` |

**Not re-hosted as Stu unit tests:** live GCS/SPPU email campaign sends (HTML/PDF folders), Raven job queue, absentee SMS *send* (recipients only), Raven cache reset.

## Run

```bash
createdb -U ct ct_modular_test
dotnet test path/to/ct.stu.tests.csproj
export CT_TEST_POSTGRES="Host=postgres;Database=ct_modular_test;Username=ct;Password=secret"
dotnet test
```

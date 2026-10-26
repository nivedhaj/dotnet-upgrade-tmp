# Ct.Gr Tests

MSTest suite for **Ct.Gr** (general receipts) using **EF Core + Postgres**.

## Coverage map (legacy → modular)

| Original | New |
|----------|-----|
| ReceiptTests (create/get/print/cancel/activate/update/order items/monthwise) | `ReceiptTests.cs` |
| ReportTests (report, summary, consolidated, revenue by class/person, print) | `ReportTests.cs` |
| EtlTests | `EtlTests.cs` (JSON import via CreateAsync) |
| IndexTests | `IndexTests.cs` (unique Number per OwnerId) |
| PingTests | `PingTests.cs` |

## Run

```bash
createdb -U ct ct_modular_test
dotnet test path/to/ct.gr.tests.csproj

export CT_TEST_POSTGRES="Host=postgres;Database=ct_modular_test;Username=ct;Password=secret"
dotnet test
```

Each test uses a unique `OwnerId` and deletes its receipts on cleanup.

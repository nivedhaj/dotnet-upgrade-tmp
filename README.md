# CT Modular Platform — Source + Tests

Combined package of the **latest modular .NET modules** (EF Core + PostgreSQL) and **MSTest suites** ported from the legacy RavenDB test zips.

## Layout

```
ct-modular/
├── Ct.Modular.sln
├── docker-compose.yml          # Postgres 16
├── README.md
├── scripts/
│   ├── setup-db.sh             # start Postgres + ensure DBs
│   ├── build-all.sh            # restore + build solution
│   ├── run-host.sh             # run API host (Swagger)
│   ├── run-tests.sh            # run all or one test project
│   └── init-db.sql
├── src/
│   ├── Ct.Host/                # combined API host
│   ├── Ct.Shared/ / Ct.Contracts/
│   └── Modules/
│       ├── Ct.Connect/
│       ├── Ct.Fee/
│       ├── Ct.Gr/
│       ├── Ct.Stu/
│       ├── Ct.Staff/
│       └── Ct.Scheduler/
└── tests/
    ├── ct.connect.tests/
    ├── ct.fee.tests/
    ├── ct.gr.tests/
    ├── ct.stu.tests/
    ├── ct.staff.tests/
    └── ct.scheduler.tests/
```

## Prerequisites

- **.NET 10 SDK** (`dotnet --version` should be 10.x)
- **Docker** (for Postgres) *or* a local Postgres 14+ instance
- Optional: `psql` client

## 1. Database

```bash
# Docker (recommended)
./scripts/setup-db.sh

# Or local Postgres
createdb -U ct ct_modular
createdb -U ct ct_modular_test
```

Default connection strings:

| Use | Connection string |
|-----|-------------------|
| **App / Host** | `Host=localhost;Port=5432;Database=ct_modular;Username=ct;Password=ct_dev_password` |
| **Tests** | `Host=localhost;Port=5432;Database=ct_modular_test;Username=ct;Password=ct_dev_password` |

Override for tests:

```bash
export CT_TEST_POSTGRES="Host=...;Database=ct_modular_test;Username=...;Password=..."
```

## 2. Build

```bash
./scripts/build-all.sh
# or
dotnet build Ct.Modular.sln -c Release
```

## 3. Run API host (services)

```bash
./scripts/setup-db.sh          # if not already running
./scripts/run-host.sh
```

- Base URL: `http://localhost:5001`
- Swagger (Development): `http://localhost:5001/swagger`

Modules register controllers under paths such as:

| Module | Typical routes |
|--------|----------------|
| Connect | `/api/connect/...` |
| Fee | `/api/fee/...` |
| Gr | `/api/gr/...` |
| Stu | `/api/stu/...` |
| Staff | `/api/staff/...` |
| Scheduler | `/api/scheduler/...` |

Sample `.http` files live under each `tests/ct.*.tests/http/`.

## 4. Run tests

```bash
./scripts/setup-db.sh
./scripts/run-tests.sh              # all suites
./scripts/run-tests.sh fee          # one module: connect|fee|gr|stu|staff|scheduler
```

Or directly:

```bash
export CT_TEST_POSTGRES="Host=localhost;Port=5432;Database=ct_modular_test;Username=ct;Password=ct_dev_password"
dotnet test tests/ct.fee.tests/ct.fee.tests.csproj
dotnet test Ct.Modular.sln --filter "FullyQualifiedName~Ct.Fee.Tests"
```

### Test design (all suites)

- **EF Core + Npgsql** (real Postgres; not InMemory)
- **Tenant isolation** via unique `OwnerId` / `InstId` per test
- **Cleanup** deletes rows created by that test
- External brokers (RabbitMQ, SMS gateways) use **logging / null stubs**

### Approximate coverage

| Suite | Focus | ~Tests |
|-------|--------|-------:|
| `ct.connect.tests` | Topics, SMS, workflows, reports, facets | ~30+ |
| `ct.fee.tests` | Fees, TX, refunds, suspense, reports, policy, ETL surface | ~76 |
| `ct.gr.tests` | Receipts, reports, ETL import, indexes | ~30 |
| `ct.stu.tests` | Students, reports, ETL, email recipients | ~81 |
| `ct.staff.tests` | Staff, employment, salary, reports, ETL | ~21 |
| `ct.scheduler.tests` | Calendar rules/events, due dates, cron helpers, reports | ~25 |

## 5. Environment variables

| Variable | Purpose |
|----------|---------|
| `CT_TEST_POSTGRES` | Test DB connection (preferred for tests) |
| `ConnectionStrings__Postgres` | Host + modules |
| `ASPNETCORE_URLS` | Host listen URLs (default `http://localhost:5001`) |
| `ASPNETCORE_ENVIRONMENT` | `Development` enables Swagger |

## Troubleshooting

- **Connection refused**: run `./scripts/setup-db.sh` and confirm `docker compose ps`
- **Schema errors**: tests call `EnsureCreated()`; drop `ct_modular_test` and re-run if models change
- **Project reference errors**: open `Ct.Modular.sln` from this folder so paths `src/Modules/...` resolve
- **Missing modules in host**: only Connect, Fee, Gr, Stu, Staff, Scheduler sources are included in this combined pack; other modules from full `src.zip` can be copied into `src/Modules/` the same way

## License / origin

Tests were generated against the modular ports of legacy `ct.*.tests` RavenDB suites. Source modules come from the provided `src.zip` (modular .NET 10 architecture).

# Ct.Campus — Student/Fees PostgreSQL + .NET 10 migration

This version removes the RavenDB dependency from the uploaded Campus Student module and moves persistence to PostgreSQL.

## Target stack

- .NET 10
- C# 14 (`LangVersion=14.0`)
- PostgreSQL
- Npgsql 10.0.3
- Autofac 9.3.1 / Autofac.Extensions.DependencyInjection 11.0.2
- ASP.NET Core 10
- NLog 6.1.4
- Swashbuckle 10.2.3

## Database design

The migration uses a PostgreSQL-first read/write model:

- `campus.documents` stores complex Campus aggregates as JSONB so the existing domain model can be migrated without losing nested Student/Course/IEP data.
- `campus.fees_receivables` and `campus.opening_balances` are normalized relational tables.
- PostgreSQL views replace RavenDB indexes:
  - `v_student_inst_index`
  - `v_student_coursewise_count`
  - `v_inst_student_count`
  - `v_attendance_event_index`
  - `v_student_essentials_index`
  - `v_ping_index`
  - `v_student_fee_summary`
- PostgreSQL triggers replace RavenDB ETL/audit hooks:
  - audit timestamps/user IDs on writes
  - fee-summary cache refresh after fee/opening-balance changes

The SQL is in `Database/schema.sql`.

## Configuration

Set:

```json
{
  "Postgres": {
    "ConnectionString": "Host=localhost;Port=5432;Database=campustrack;Username=campustrack;Password=CHANGE_ME",
    "ApplySchemaOnStartup": true
  }
}
```

For production, run `Database/schema.sql` through your deployment/migration pipeline and set `ApplySchemaOnStartup` to `false`.

## Compatibility layer

`Ct.Core/Data/PostgresSession.cs` provides the small persistence abstraction needed to keep the existing Student command/query logic stable while removing RavenDB APIs.

It supports:

- `Query<T>()`
- `Query<T,TIndex>()`
- `Load<T>()`
- `Load<T>(ids)`
- `Store/StoreAsync`
- `SaveChanges/SaveChangesAsync`
- stream-style reads
- the existing statistics/search/document-query patterns

The important difference is that the database is now PostgreSQL; the old RavenDB client package and RavenDB index/ETL APIs are gone.

## Fee summary

The previous RavenDB ETL sketch has been replaced with PostgreSQL triggers:

`fees_receivables/opening_balances -> trigger -> student_fee_summary_cache -> v_student_fee_summary`

This makes the fee read model local to PostgreSQL and removes the cross-database Raven ETL dependency.

## Build

The environment used to prepare this migration does not contain the .NET SDK, so the package cannot be build-verified here.

On a machine with the .NET 10 SDK:

```bash
dotnet restore
dotnet build
dotnet run --project src/Ct.Host
```

Before production rollout, validate the PostgreSQL schema against a copy of the existing Raven data and run the Student/Fees integration test suite.

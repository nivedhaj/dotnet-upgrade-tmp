# ct.stu — Full .NET 10 / C# 14 / PostgreSQL migration

This package is the full ct.stu application source migrated from the supplied legacy `ct.stu` source.

## Included

- All original `Controllers/` endpoints, including `StudentController` and `ReportController`.
- All original student commands in `Repo/Db/Command/StudentCmd.cs`.
- All original student queries in `Repo/Db/Query/StudentQry.cs`.
- Report service and report controller behavior.
- DTOs and resource/domain classes.
- Policy engines, middleware, security/user-context plumbing and integrations.
- PostgreSQL persistence replacing RavenDB.
- PostgreSQL JSONB document storage so the existing aggregate model is preserved without losing nested student data.
- PostgreSQL views and indexes for students, enrollments and attendance.
- Replacements for the Raven indexes used by ct.stu:
  - `Student_Inst_Index`
  - `Student_Coursewise_Count`
  - `Inst_Student_Count`
  - `AttendanceEvent_IX`
  - `Student_Essentials_IX`
  - `Ping_Index`
- Docker Compose with PostgreSQL.
- Database schema under `Database/schema.sql`.

## RavenDB status

There is no RavenDB client package, RavenDB connection, Raven `IDocumentSession`, or Raven index deployment code in the migrated runtime.

The legacy query/index semantics are implemented through `ct._Core.Persistence.PostgresDocumentSession` and PostgreSQL-backed document/index projections.

## Run

```bash
docker compose up --build
```

The API is exposed on port `5190` and PostgreSQL on `5432`.

Swagger:

```text
http://localhost:5190/api/stu/swagger
```

## Configuration

Set:

```text
ConnectionStrings__Postgres=Host=postgres;Port=5432;Database=ctstu;Username=ctstu;Password=ctstu
```

For local non-Docker execution, use `Host=localhost`.

## Database

The application automatically runs the idempotent PostgreSQL schema initializer at startup. The same SQL is available in `Database/schema.sql`.

The schema uses a JSONB aggregate/document table because the legacy ct.stu model contains deeply nested objects, arrays and polymorphic aggregates. Relational metadata and targeted PostgreSQL indexes are maintained alongside the JSONB payload.

## Important migration note

The source migration is complete at the application-code level: controllers, commands, queries, reports and supporting services are present. Because this execution environment does not contain the .NET 10 SDK, the archive has not been compiled or integration-tested here. Run the following on a machine with the .NET 10 SDK and NuGet access before production cutover:

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

Then run endpoint regression tests against a PostgreSQL database populated from the Raven export.

## Data migration

The application schema is ready for imported aggregates. A Raven export/import step must map each legacy document into `ct_documents(document_type,id,owner_id,parent_id,created_on,modified_on,data)` and then reconcile document counts and business-level totals before cutover.

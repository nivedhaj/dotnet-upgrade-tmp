# ct.stu — .NET 10 / EF Core / PostgreSQL migration

This is a **partial, working implementation** built from
`ct-stu-standalone-agent-migration-spec.md`, covering the Admissions/Student
core vertical (Person, Student, Enrollment, Promotion, Course structure) in
full, with the remaining modules (Attendance, Fees, Examinations, IEP,
Essentials, Documents, Baseline, Reports) intentionally left unimplemented
rather than stubbed with fake logic.

**This code has not been compiled or run.** The environment that produced it
has no .NET SDK and no PostgreSQL instance. Treat everything here as a
first-pass draft that needs real `dotnet build` / `dotnet test` /
`docker compose up` verification before you trust it. See
`docs/migration-matrix.md` for exactly what's implemented, tested, or
missing, and for the assumptions that were made where the spec didn't fully
determine legacy behavior.

## Quick start (once you have .NET 10 SDK + Docker installed)

```bash
# 1. Restore & build
dotnet restore ct.csproj
dotnet build ct.csproj

# 2. Run unit + integration tests (integration tests use EF InMemory —
#    no live Postgres required for these)
dotnet test tests/ct.Tests.csproj

# 3. Bring up Postgres + apply schema
docker compose up -d postgres
psql "postgresql://ct:ct_dev_password@localhost:5432/ct" -f database/001_schema.sql
psql "postgresql://ct:ct_dev_password@localhost:5432/ct" -f database/002_indexes.sql
psql "postgresql://ct:ct_dev_password@localhost:5432/ct" -f database/003_views.sql
psql "postgresql://ct:ct_dev_password@localhost:5432/ct" -f database/004_triggers.sql

# 4. Run the app
dotnet run --project ct.csproj
# Swagger at http://localhost:5000/swagger (or whatever port dotnet run binds)

# 5. Or run everything in Docker
docker compose up --build
```

## What's implemented
- Full Person/Student/Enrollment/Course domain model with legacy business
  rules preserved (name normalization, virtual id derivation, enrollment
  lifecycle, promotion, course section/subject rules).
- EF Core mapping (`CtDbContext` + per-entity configurations) targeting
  PostgreSQL/Npgsql, JSONB for genuinely dynamic fields only.
- Hand-written SQL schema/indexes/views/triggers for the above.
- `StudentsController` implementing 13 of the legacy routes end-to-end
  (create, enable/disable, promote, search, get, tags, roll number, course
  update, primary contact, flat data, by-tag).
- Docker/Compose, health check, Swagger, exception-handling middleware,
  NLog config.
- Unit tests for every documented business rule in the delivered scope, plus
  integration tests against EF InMemory for the command/query services.

## What's not
See `docs/migration-matrix.md` — Attendance, Fees, Examinations, IEP,
Essentials, Documents, Baseline, Reports, RabbitMQ/Redis/S3/GitHub
integrations, and most of the remaining ~45 legacy routes.

## Continuing the migration
Follow the phase order in the original spec (sections 119, F onward):
Contacts/Parents/Tags completion → Academics/Courses API → Attendance →
Fees → IEP/Essentials/Documents/Baseline → Reports → external services →
full test suite → Dockerize → Raven-dependency audit. Each phase should get
its own real `dotnet build && dotnet test` run before moving on — don't
batch verification to the end.

# CT Modular Platform

A **modular structured .NET 10 + PostgreSQL application** that combines the previously independent migration modules into one cohesive solution.

## Modules

| Module       | Original package                        | Route / schema      | Content status |
|--------------|-----------------------------------------|---------------------|----------------|
| **Ct.Fee**   | ct-fee-dotnet10-postgres-production     | `/api/fee/*`        | Full (Domain, App, Infra, Controllers) |
| **Ct.Stu**   | ct-stu-migration                        | Students / `ct`     | Full Admissions vertical (Person, Student, Enrollment, Course) |
| **Ct.Acad**  | ct.acad.net10-migration                 | `/api/acad/*`       | Ping module + infrastructure + consolidated AcadModule |
| **Ct.Arte**  | ct-arte-migration-dotnet10-postgres     | Artefacts           | Full (Artefact domain, repository, service, DbContext) |
| **Ct.Connect**| ct.connect.pg-migration                | Connect             | Full (Email/SMS/Topic domain, Cmd/Qry, DbContext) |
| **Ct.Library**| ct-library-postgres-migration          | Materials (Marten)  | Full (event-sourced Material, Marten, projections, queries) |

## Architecture

```
CtModularApp/
├── Ct.Modular.sln
├── Dockerfile
├── docker-compose.yml
├── README.md
└── src/
    ├── Ct.Host/                 # Composition root
    ├── Ct.Shared/               # IUserContext, AuditableEntity, extensions
    └── Modules/
        ├── Ct.Fee/
        ├── Ct.Stu/              # Core + Modules/Student (Admissions)
        ├── Ct.Acad/             # Application, Controllers, Domain, Infrastructure, _Core
        ├── Ct.Arte/
        ├── Ct.Connect/
        └── Ct.Library/          # Domain, Handlers, Infrastructure (Marten), Queries, ReadModels
```

### Design principles

1. **Modular monolith** – single deployable host, clear module boundaries.
2. **Shared kernel** (`Ct.Shared`) – common abstractions where useful.
3. **Module registration** – each module exposes `AddXxxModule` + optional `MapXxxEndpoints`.
4. **Independent persistence** – each module owns its DbContext / Marten store and schema.
5. **Header-based tenant/user context** (`X-Owner-Id`, `X-User-Id`, …).
6. **Stack**: .NET 10, C# 14, EF Core 10, Npgsql, Marten (Library).

## Run

```bash
# Requires .NET 10 SDK + Docker
cd CtModularApp
dotnet restore
dotnet build
docker compose up -d postgres   # or full stack
dotnet run --project src/Ct.Host
```

- Swagger: `/swagger`
- Health: `/health`

Every API call should include tenant/user headers where required:

```text
X-Owner-Id: <guid>
X-User-Id: <user-id>
```

## Notes

- **Fee** and **Arte** use EF Core + PostgreSQL with their own DbContexts.
- **Stu** uses a single `CtDbContext` with schema `ct` and the Admissions module.
- **Acad** uses schema `acad` and a Ping vertical; a large consolidated `AcadModule.Consolidated.cs` is also present for reference.
- **Connect** uses `ConnectDbContext` (Email / SMS / Topic).
- **Library** uses **Marten** for event-sourced Material aggregates and projections.
- Controllers are auto-discovered by the host’s `MapControllers()`.

## Next steps (optional)

- Unify user-context interfaces onto `Ct.Shared.Abstractions.IUserContext`.
- Add integration / golden tests per module.
- Feature flags via `Modules:Xxx:Enabled`.
- Production Marten migrations instead of `AutoCreate.All`.

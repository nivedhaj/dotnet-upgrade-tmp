# Ct.Campus — .NET 10 / C# 14 + PostgreSQL (Marten)

Full original **Student** module tree upgraded:

- **Controllers** kept (`StudentController`, `ReportController`, `FeesController`, …)
- **Views** kept (`Student/Views/StudentFeeSummary.cs`)
- **Triggers** kept & rewritten (`Fees/Triggers/FeeSummaryEtl.cs` → Marten publish)
- **Services / Cmd / Qry / DTOs / Resources** kept
- **RavenDB** replaced with **Marten on PostgreSQL** (document model on Postgres)
- Target framework **`net10.0`**, language **C# 14**

## Why Marten?

Your code is document-oriented (sessions, `Store`/`Load`/`Query`, nested documents, view collections).  
Marten stores documents as **PostgreSQL `jsonb`** and exposes an API close to Raven’s session model, so Controllers, Views, and most Cmd/Qry call sites can stay.

## Structure (preserved)

```
src/
  Ct.Host/          # executable host (.NET 10)
  Ct.Core/          # shared security, middleware, compat shims
  Ct.Contracts/
  Student/
    Controllers/
    Views/          # StudentFeeSummary  ← VIEW
    Triggers/       # README for trigger/view pattern
    Repo/ Db/ Service/
    StudentModule.cs
  Fees/
    Controllers/
    Triggers/       # FeeSummaryEtl       ← TRIGGER (Postgres/Marten)
    Repo/
    FeesModule.cs
```

## Setup

1. PostgreSQL 14+ running.
2. Create databases (or let Marten create schema objects):

```bash
psql -U postgres -c "CREATE DATABASE campus_student;"
psql -U postgres -c "CREATE DATABASE campus_fees;"
```

3. Connection strings in `src/Ct.Host/appsettings.Development.json`:

```json
"ConnectionStrings": {
  "Student": "Host=localhost;Port=5432;Database=campus_student;Username=postgres;Password=postgres",
  "Fees": "Host=localhost;Port=5432;Database=campus_fees;Username=postgres;Password=postgres"
}
```

4. Build & run:

```bash
dotnet restore Ct.Campus.sln
dotnet build Ct.Campus.sln
dotnet run --project src/Ct.Host
```

## View + Trigger

| Piece | File | Behavior |
|-------|------|----------|
| **View** | `Student/Views/StudentFeeSummary.cs` | Document in Student store; identity = `StudentId` |
| **Trigger** | `Fees/Triggers/FeeSummaryEtl.cs` | `PublishAsync` upserts the view after Fees writes |

Call from Fees after save:

```csharp
await FeeSummaryEtl.PublishAsync(studentStore, studentId, instId, receivable, paid, opening);
```

## What still needs attention

1. **Raven map indexes** under `Repo/Db/Index/*` are now Result DTO holders. Configure Marten property indexes / computed indexes in `StudentModule.CreateStore` for hot filters (`InstId`, `EnrollmentStatus`, …).
2. **`Advanced.Stream` / `ProjectInto`** use temporary **compat shims** in `Ct.Core/Compat/MartenRavenCompat.cs`. Prefer plain `.ToList()` over time.
3. **`Query<T, TIndex>`** was rewritten to **`Query<T>`**. Some projections may need explicit `.Select(...)`.
4. **Not fully compile-tested** in this environment (no full SDK restore of all transitive packages). Expect a few API fixes on first `dotnet build` (Marten version APIs, nullable, etc.).
5. Wire **Fees** into the host when ready (uncomment project reference + `FeesModule.Register`).

## Packages

- Removed: `RavenDB.Client`
- Added: `Marten` (PostgreSQL document DB)
- Host: `net10.0`, Autofac still used for keyed module registration

## Original Raven config

`Raven:Urls` removed. Use `ConnectionStrings:Student` / `Fees` only.

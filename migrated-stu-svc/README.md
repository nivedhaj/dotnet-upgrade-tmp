# ct.stu — .NET 10 / C# 14 / EF Core / PostgreSQL modular migration

This solution uses a shared `_Core` project. Individual services live below `src/Services` and **do not contain their own Core folder**.

## Structure

```text
src/
├── _Core/
│   └── Ct.Core/
└── Services/
    └── Ct.Stu/
        ├── Ct.Stu.Domain/
        ├── Ct.Stu.Application/
        ├── Ct.Stu.Infrastructure/
        └── Ct.Stu.Api/
```

## Data architecture

- EF Core 10 + Npgsql
- PostgreSQL tables are managed by EF migrations.
- Complex read models are exposed through PostgreSQL views/materialized views.
- PostgreSQL trigger/function SQL is versioned with the EF migration.
- `student_fee_summary_view` is the canonical live view.
- `student_fee_summary_mv` is an optional read-optimized materialized projection and is refreshed after fee transaction statements.
- Legacy RavenDB indexes are represented as EF/LINQ queries or PostgreSQL indexes/views where that is a better relational fit.
- JSONB is retained for legacy enrollment/contacts fields during the migration so data is not destructively flattened.

## Commands

```bash
dotnet restore
dotnet build

dotnet test

dotnet run --project src/Services/Ct.Stu/Ct.Stu.Api
```

For an existing database, generate/apply migrations with:

```bash
dotnet ef migrations add InitialPostgres --project src/Services/Ct.Stu/Ct.Stu.Infrastructure --startup-project src/Services/Ct.Stu/Ct.Stu.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/Services/Ct.Stu/Ct.Stu.Infrastructure --startup-project src/Services/Ct.Stu/Ct.Stu.Api
```

## Important migration note

`MigrationOutput.zip` was used as the Roslyn migration-analysis reference. It reports 110 scanned files and 526 detected migration points, including RavenDB `Load`, `Store`, `SaveChanges`, and `Query` usage in the shared core and student command/query paths. The original Roslyn report is included under `migration-analysis/`.

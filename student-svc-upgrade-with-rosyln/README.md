# ct.stu -> .NET 10 / C# 14 / PostgreSQL migration workspace

This workspace preserves the complete original `ct.stu` application under `Legacy/` and adds a runnable .NET 10 PostgreSQL POC plus a Roslyn migration analyzer.

## Architecture

Legacy .NET Core 3.1 + RavenDB -> Roslyn analysis -> .NET 10/C# 14 + PostgreSQL

Roslyn is used as a **migration/code-analysis bridge**, not as a runtime database adapter.

## Run with Docker

Requirements: Docker and Docker Compose.

```bash
docker compose up --build
```

API: http://localhost:8080
Swagger: http://localhost:8080/swagger
PostgreSQL: localhost:5432

To reset the database during POC work:

```bash
docker compose down -v
docker compose up --build
```

## Run Roslyn analyzer

From the repository root:

```bash
dotnet run --project src/StudentMigration.Roslyn -- Legacy/ct.stu/Repo/Db/Command/StudentCmd.cs
```

Or analyze the entire legacy project:

```bash
dotnet run --project src/StudentMigration.Roslyn -- Legacy/ct.stu
```

The analyzer reports RavenDB APIs, source locations, and suggested PostgreSQL/EF Core replacements.

## Scope

The PostgreSQL API is an intentionally small vertical slice around Student. It is the migration foundation, not a claim that every 110 legacy C# files has been mechanically rewritten. The full original source is retained so each Raven operation can be migrated and regression-tested incrementally.

## Roslyn migration report output

The Roslyn analyzer supports an optional `--output` directory. It writes both a machine-readable JSON report and a human-readable Markdown report.

```bash
dotnet run --project src/StudentMigration.Roslyn -- Legacy/ct.stu --output MigrationOutput
```

The generated files are:

```text
MigrationOutput/
├── migration-report.json
└── migration-report.md
```

The console still prints a summary and the detected migration points. The output directory is created automatically if it does not exist.

For a single file:

```bash
dotnet run --project src/StudentMigration.Roslyn -- Legacy/ct.stu/Repo/Db/Command/StudentCmd.cs --output MigrationOutput
```
